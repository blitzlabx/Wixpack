using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Wixpack.Core.Branding;
using Wixpack.Core.Configuration;
using Wixpack.DevTools.Services;
using Wixpack.Downloader.Services;
using Wixpack.Games.Core;
using Wixpack.Telegram.Floket;
using Wixpack.Telegram.Keyboards;

namespace Wixpack.Telegram.Handlers;

public sealed class CallbackQueryHandler
{
    private readonly FloketGroupHandler _floket;
    private readonly GameRegistry _games;
    private readonly DevToolsService _tools;
    private readonly MediaExtrasClient _extras;
    private readonly WixpackSettings _settings;
    private readonly ILogger<CallbackQueryHandler> _logger;

    public CallbackQueryHandler(
        FloketGroupHandler floket,
        GameRegistry games,
        DevToolsService tools,
        MediaExtrasClient extras,
        IOptions<WixpackSettings> settings,
        ILogger<CallbackQueryHandler> logger)
    {
        _floket = floket;
        _games = games;
        _tools = tools;
        _extras = extras;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task HandleAsync(ITelegramBotClient bot, CallbackQuery query, CancellationToken ct)
    {
        if (query.Data is null || query.Message is null)
            return;

        var data = query.Data;

        try
        {
            if (data.StartsWith("floket:", StringComparison.Ordinal))
            {
                await _floket.OnCallbackAsync(bot, query, ct);
                return;
            }

            if (data.StartsWith("game:start:", StringComparison.Ordinal))
            {
                var gameId = data["game:start:".Length..];
                var game = _games.Get(gameId);
                if (game is null)
                {
                    await bot.AnswerCallbackQuery(query.Id, "Unknown game", showAlert: true, cancellationToken: ct);
                    return;
                }
                await bot.AnswerCallbackQuery(query.Id, cancellationToken: ct);
                await game.StartAsync(bot, query.Message.Chat.Id, query.From?.Id ?? 0, ct);
                return;
            }

            if (data.StartsWith("game:", StringComparison.Ordinal))
            {
                var rest = data["game:".Length..];
                var idx = rest.IndexOf(':');
                if (idx > 0)
                {
                    var gameId = rest[..idx];
                    var payload = rest[(idx + 1)..];
                    var game = _games.Get(gameId);
                    if (game is not null)
                        await game.OnCallbackAsync(bot, query, payload, ct);
                }
                return;
            }

            var chatId = query.Message.Chat.Id;
            var messageId = query.Message.MessageId;

            switch (data)
            {
                case "menu:main":
                    await bot.EditMessageText(chatId, messageId,
                        $"<b>{WixpackBranding.FullProductName}</b>\nPick a feature:",
                        parseMode: ParseMode.Html,
                        replyMarkup: WixpackKeyboards.MainMenu(), cancellationToken: ct);
                    break;

                case "menu:about":
                    await bot.EditMessageText(chatId, messageId, WixpackKeyboards.AboutText(),
                        parseMode: ParseMode.Html, replyMarkup: WixpackKeyboards.About(), cancellationToken: ct);
                    break;

                case "menu:close":
                    await bot.DeleteMessage(chatId, messageId, cancellationToken: ct);
                    break;

                case "menu:floket":
                    await bot.EditMessageText(chatId, messageId,
                        "<b>🔐 Floket</b>\n\n" +
                        "Group human verification with sessions, expiry, attempt limits and restrict-until-verified.\n\n" +
                        "1. Add bot to group\n2. Admin + Restrict members\n3. New joins get a challenge\n\n" +
                        $"Timeout: <code>{_settings.Floket.VerificationTimeoutSeconds}s</code>\n" +
                        $"Max attempts: <code>{_settings.Floket.MaxAttempts}</code>",
                        parseMode: ParseMode.Html, replyMarkup: WixpackKeyboards.BackOnly(), cancellationToken: ct);
                    break;

                case "menu:games":
                    var list = string.Join("\n", _games.All.Select(g => $"• <b>{g.DisplayName}</b> — {g.Description}"));
                    await bot.EditMessageText(chatId, messageId,
                        $"🎮 <b>Games</b>\n\n{list}\n\nOr <code>/game rps|guess|dice</code>",
                        parseMode: ParseMode.Html, replyMarkup: WixpackKeyboards.GamesMenu(), cancellationToken: ct);
                    break;

                case "menu:downloader":
                    await bot.EditMessageText(chatId, messageId,
                        "📥 <b>Downloader</b>\n\n<code>/dl https://...</code>\n\n" +
                        "YouTube · TikTok · Instagram · X · Facebook · Spotify · SoundCloud · AIO fallback\n\n" +
                        "Private chat: paste a URL alone.",
                        parseMode: ParseMode.Html, replyMarkup: WixpackKeyboards.DownloaderMenu(), cancellationToken: ct);
                    break;

                case "menu:devtools":
                    await bot.EditMessageText(chatId, messageId,
                        "🛠 <b>Developer Tools</b>\n\nButtons below · HTTP under <code>/api/tools/*</code>",
                        parseMode: ParseMode.Html, replyMarkup: WixpackKeyboards.DevToolsMenu(), cancellationToken: ct);
                    break;

                case "menu:extras":
                    await bot.EditMessageText(chatId, messageId,
                        "🎲 <b>Extras</b>\n\nRandom media, quick quiz and more.",
                        parseMode: ParseMode.Html, replyMarkup: WixpackKeyboards.ExtrasMenu(), cancellationToken: ct);
                    break;

                case "menu:settings":
                    var logo = string.IsNullOrWhiteSpace(_settings.LogoUrl) ? "(not set)" : _settings.LogoUrl;
                    var donation = string.IsNullOrWhiteSpace(_settings.DonationUrl) ? "(not set)" : _settings.DonationUrl;
                    await bot.EditMessageText(chatId, messageId,
                        $"⚙️ <b>Settings</b>\n\n" +
                        $"<b>{WixpackBranding.FullProductName}</b>\n" +
                        $"Creator: {WixpackBranding.Creator} · @{WixpackBranding.SocialHandle}\n\n" +
                        $"Logo: <code>{Escape(logo)}</code>\nDonation: <code>{Escape(donation)}</code>\n\n" +
                        $"Token source: <b>environment / .env</b>\n" +
                        $"Floket: {_settings.Floket.VerificationTimeoutSeconds}s · {_settings.Floket.MaxAttempts} attempts",
                        parseMode: ParseMode.Html,
                        replyMarkup: WixpackKeyboards.SettingsMenu(_settings.DonationUrl), cancellationToken: ct);
                    break;

                case "tool:uuid":
                    await bot.AnswerCallbackQuery(query.Id, cancellationToken: ct);
                    await bot.SendMessage(chatId, $"🆔 <code>{_tools.NewGuid()}</code>", parseMode: ParseMode.Html, cancellationToken: ct);
                    return;

                case "tool:uuid5":
                    await bot.AnswerCallbackQuery(query.Id, cancellationToken: ct);
                    var ids = string.Join("\n", _tools.BulkUuid(5).Select(u => $"<code>{u}</code>"));
                    await bot.SendMessage(chatId, $"🆔 UUIDs\n{ids}", parseMode: ParseMode.Html, cancellationToken: ct);
                    return;

                case "tool:timestamp":
                    await bot.AnswerCallbackQuery(query.Id, cancellationToken: ct);
                    var now = DateTimeOffset.UtcNow;
                    await bot.SendMessage(chatId,
                        $"⏱ <b>UTC</b>\nUnix: <code>{now.ToUnixTimeSeconds()}</code>\nISO: <code>{now:O}</code>",
                        parseMode: ParseMode.Html, cancellationToken: ct);
                    return;

                case "tool:password":
                    await bot.AnswerCallbackQuery(query.Id, cancellationToken: ct);
                    await bot.SendMessage(chatId, $"🔑 <code>{_tools.GeneratePassword(20)}</code>", parseMode: ParseMode.Html, cancellationToken: ct);
                    return;

                case "tool:coinflip":
                    await bot.AnswerCallbackQuery(query.Id, Random.Shared.Next(2) == 0 ? "Heads" : "Tails", showAlert: true, cancellationToken: ct);
                    return;

                case "tool:lorem":
                    await bot.AnswerCallbackQuery(query.Id, cancellationToken: ct);
                    await bot.SendMessage(chatId, _tools.Lorem(40), cancellationToken: ct);
                    return;

                case "tool:stats":
                    await bot.AnswerCallbackQuery(query.Id, cancellationToken: ct);
                    await bot.SendMessage(chatId, "📊 Demo: length=24 words=4 letters=20", cancellationToken: ct);
                    return;

                case "tool:color":
                    await bot.AnswerCallbackQuery(query.Id, cancellationToken: ct);
                    var col = _tools.ColorConvert("#3B82F6");
                    await bot.SendMessage(chatId,
                        col.Success ? $"🎨 Demo #3B82F6 → {JsonSerializer.Serialize(col.Value)}" : col.Error!,
                        cancellationToken: ct);
                    return;

                case "extra:cat":
                case "extra:dog":
                case "extra:quiz":
                    await bot.AnswerCallbackQuery(query.Id, "Loading…", cancellationToken: ct);
                    await HandleExtraAsync(bot, chatId, data, ct);
                    return;

                default:
                    _logger.LogDebug("Unhandled callback: {Data}", data);
                    break;
            }

            await bot.AnswerCallbackQuery(query.Id, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Callback failed: {Data}", data);
            try { await bot.AnswerCallbackQuery(query.Id, "Error", showAlert: true, cancellationToken: ct); } catch { }
        }
    }

    private async Task HandleExtraAsync(ITelegramBotClient bot, long chatId, string data, CancellationToken ct)
    {
        var result = data switch
        {
            "extra:cat" => await _extras.RandomCatAsync(ct),
            "extra:dog" => await _extras.RandomDogAsync(ct),
            _ => await _extras.QuizRandomAsync(ct)
        };

        if (!result.Success)
        {
            await bot.SendMessage(chatId, $"❌ {result.Error}", cancellationToken: ct);
            return;
        }

        var json = result.Value.ToString();
        if (json.Length > 3500) json = json[..3500] + "…";
        await bot.SendMessage(chatId, $"<pre>{Escape(json)}</pre>", parseMode: ParseMode.Html, cancellationToken: ct);
    }

    private static string Escape(string s) =>
        s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
}

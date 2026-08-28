using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Wixpack.Core.Branding;
using Wixpack.Core.Configuration;
using Wixpack.Games.Core;
using Wixpack.Telegram.Floket;
using Wixpack.Telegram.Keyboards;

namespace Wixpack.Telegram.Handlers;

public sealed class CallbackQueryHandler
{
    private readonly FloketGroupHandler _floket;
    private readonly GameRegistry _games;
    private readonly WixpackSettings _settings;
    private readonly ILogger<CallbackQueryHandler> _logger;

    public CallbackQueryHandler(
        FloketGroupHandler floket,
        GameRegistry games,
        IOptions<WixpackSettings> settings,
        ILogger<CallbackQueryHandler> logger)
    {
        _floket = floket;
        _games = games;
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

            // Start a game from menu: game:start:{id}
            if (data.StartsWith("game:start:", StringComparison.Ordinal))
            {
                var gameId = data["game:start:".Length..];
                var game = _games.Get(gameId);
                if (game is null)
                {
                    await bot.AnswerCallbackQuery(query.Id, "Unknown game", showAlert: true, cancellationToken: ct);
                    return;
                }

                var userId = query.From?.Id ?? 0;
                await bot.AnswerCallbackQuery(query.Id, cancellationToken: ct);
                await game.StartAsync(bot, query.Message.Chat.Id, userId, ct);
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
                    await bot.EditMessageText(
                        chatId, messageId,
                        $"<b>{WixpackBranding.FullProductName}</b>\nMain menu — pick a feature:",
                        parseMode: ParseMode.Html,
                        replyMarkup: WixpackKeyboards.MainMenu(),
                        cancellationToken: ct);
                    break;

                case "menu:about":
                    await bot.EditMessageText(
                        chatId, messageId,
                        WixpackKeyboards.AboutText(),
                        parseMode: ParseMode.Html,
                        replyMarkup: WixpackKeyboards.About(),
                        cancellationToken: ct);
                    break;

                case "menu:close":
                    await bot.DeleteMessage(chatId, messageId, cancellationToken: ct);
                    break;

                case "menu:floket":
                    await bot.EditMessageText(
                        chatId, messageId,
                        "<b>🔐 Floket Human Verification</b>\n\n" +
                        "When enabled in a group, new members must pass a Floket challenge before they can post.\n\n" +
                        "1. Add this bot to your group\n" +
                        "2. Promote it to admin with <b>Restrict members</b>\n" +
                        "3. New joins are challenged automatically\n\n" +
                        $"Timeout: <code>{_settings.Floket.VerificationTimeoutSeconds}s</code>\n" +
                        $"Max attempts: <code>{_settings.Floket.MaxAttempts}</code>\n\n" +
                        "<i>Powered by Floket</i>",
                        parseMode: ParseMode.Html,
                        replyMarkup: WixpackKeyboards.BackOnly(),
                        cancellationToken: ct);
                    break;

                case "menu:games":
                    var gameList = string.Join("\n", _games.All.Select(g =>
                        $"• <b>{g.DisplayName}</b> — {g.Description}"));
                    await bot.EditMessageText(
                        chatId, messageId,
                        $"🎮 <b>Games</b>\n\n{gameList}\n\n" +
                        "Or use <code>/game rps</code> in any chat.",
                        parseMode: ParseMode.Html,
                        replyMarkup: WixpackKeyboards.GamesMenu(),
                        cancellationToken: ct);
                    break;

                case "menu:downloader":
                    await bot.EditMessageText(
                        chatId, messageId,
                        "📥 <b>Social Downloader</b>\n\n" +
                        "Send a link with:\n<code>/dl https://...</code>\n\n" +
                        "Supported:\n" +
                        "• YouTube\n• TikTok\n• Instagram\n• X / Twitter\n" +
                        "• Facebook\n• Spotify\n• SoundCloud\n• and more via AIO\n\n" +
                        "You can also paste a URL alone in a private chat with the bot.",
                        parseMode: ParseMode.Html,
                        replyMarkup: WixpackKeyboards.DownloaderMenu(),
                        cancellationToken: ct);
                    break;

                case "menu:devtools":
                    await bot.EditMessageText(
                        chatId, messageId,
                        "🛠 <b>Developer Tools</b>\n\n" +
                        "Quick tools below, or use the HTTP API:\n" +
                        "<code>POST /api/tools/hash</code>\n" +
                        "<code>POST /api/tools/base64/encode</code>\n" +
                        "<code>POST /api/tools/qr</code>\n" +
                        "<code>GET  /api/tools/uuid</code>",
                        parseMode: ParseMode.Html,
                        replyMarkup: WixpackKeyboards.DevToolsMenu(),
                        cancellationToken: ct);
                    break;

                case "menu:settings":
                    var logo = string.IsNullOrWhiteSpace(_settings.LogoUrl) ? "(not set)" : _settings.LogoUrl;
                    var donation = string.IsNullOrWhiteSpace(_settings.DonationUrl) ? "(not set)" : _settings.DonationUrl;
                    await bot.EditMessageText(
                        chatId, messageId,
                        $"⚙️ <b>Settings</b>\n\n" +
                        $"Product: <b>{WixpackBranding.FullProductName}</b>\n" +
                        $"Creator: {WixpackBranding.Creator} · @{WixpackBranding.SocialHandle}\n\n" +
                        $"Logo: <code>{Escape(logo)}</code>\n" +
                        $"Donation: <code>{Escape(donation)}</code>\n\n" +
                        $"Floket timeout: {_settings.Floket.VerificationTimeoutSeconds}s\n" +
                        $"Floket max attempts: {_settings.Floket.MaxAttempts}",
                        parseMode: ParseMode.Html,
                        replyMarkup: WixpackKeyboards.SettingsMenu(_settings.DonationUrl),
                        cancellationToken: ct);
                    break;

                case "tool:uuid":
                    await bot.AnswerCallbackQuery(query.Id, cancellationToken: ct);
                    await bot.SendMessage(
                        chatId,
                        $"🆔 <code>{Guid.NewGuid()}</code>",
                        parseMode: ParseMode.Html,
                        cancellationToken: ct);
                    return;

                case "tool:timestamp":
                    var now = DateTimeOffset.UtcNow;
                    await bot.AnswerCallbackQuery(query.Id, cancellationToken: ct);
                    await bot.SendMessage(
                        chatId,
                        $"⏱ <b>UTC now</b>\n" +
                        $"Unix: <code>{now.ToUnixTimeSeconds()}</code>\n" +
                        $"ISO: <code>{now:O}</code>",
                        parseMode: ParseMode.Html,
                        cancellationToken: ct);
                    return;

                case "tool:coinflip":
                    var side = Random.Shared.Next(0, 2) == 0 ? "Heads" : "Tails";
                    await bot.AnswerCallbackQuery(query.Id, side, showAlert: true, cancellationToken: ct);
                    return;

                default:
                    _logger.LogDebug("Unhandled callback: {Data}", data);
                    break;
            }

            await bot.AnswerCallbackQuery(query.Id, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Callback handling failed for {Data}", data);
            try
            {
                await bot.AnswerCallbackQuery(query.Id, "Something went wrong", showAlert: true, cancellationToken: ct);
            }
            catch { /* ignore */ }
        }
    }

    private static string Escape(string s) =>
        s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
}

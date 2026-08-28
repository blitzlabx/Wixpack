using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Wixpack.Games.Core;
using Wixpack.Telegram.Floket;
using Wixpack.Telegram.Keyboards;

namespace Wixpack.Telegram.Handlers;

public sealed class CallbackQueryHandler
{
    private readonly FloketGroupHandler _floket;
    private readonly GameRegistry _games;
    private readonly ILogger<CallbackQueryHandler> _logger;

    public CallbackQueryHandler(FloketGroupHandler floket, GameRegistry games, ILogger<CallbackQueryHandler> logger)
    {
        _floket = floket;
        _games = games;
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

            if (data.StartsWith("game:", StringComparison.Ordinal))
            {
                // game:{gameId}:{payload...}
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
                        "Main menu — pick a feature:",
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
                        "<b>Floket Human Verification</b>\n\n" +
                        "When enabled in a group, new members must pass a Floket challenge before they can post.\n\n" +
                        "Admins: add the bot as admin with <b>Restrict members</b> permission, then new joins are protected automatically.\n\n" +
                        "<i>🔐 Powered by Floket</i>",
                        parseMode: ParseMode.Html,
                        replyMarkup: WixpackKeyboards.About(),
                        cancellationToken: ct);
                    break;

                case "menu:devtools":
                case "menu:games":
                case "menu:downloader":
                case "menu:settings":
                    await bot.EditMessageText(
                        chatId, messageId,
                        $"⏳ <b>{data["menu:".Length..]}</b> module loads in a later section.\nUse /start for the menu.",
                        parseMode: ParseMode.Html,
                        replyMarkup: WixpackKeyboards.About(),
                        cancellationToken: ct);
                    break;

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
}

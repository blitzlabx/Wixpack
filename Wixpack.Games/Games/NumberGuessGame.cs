using System.Security.Cryptography;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Wixpack.Games.Core;

namespace Wixpack.Games.Games;

public sealed class NumberGuessGame : IGame
{
    private readonly IGameSessionStore _store;
    public NumberGuessGame(IGameSessionStore store) => _store = store;

    public string Id => "guess";
    public string DisplayName => "Number Guess";
    public string Description => "Guess a number from 1 to 10.";

    public async Task StartAsync(ITelegramBotClient bot, long chatId, long starterUserId, CancellationToken ct)
    {
        var sessionId = Convert.ToHexString(RandomNumberGenerator.GetBytes(6)).ToLowerInvariant();
        var secret = Random.Shared.Next(1, 11).ToString();
        var session = new GameSession
        {
            SessionId = sessionId,
            GameId = Id,
            ChatId = chatId,
            HostUserId = starterUserId
        };
        session.State["secret"] = secret;
        session.State["tries"] = "0";
        await _store.SaveAsync(session, ct);

        var row1 = Enumerable.Range(1, 5).Select(n =>
            InlineKeyboardButton.WithCallbackData(n.ToString(), $"game:guess:{sessionId}:{n}")).ToArray();
        var row2 = Enumerable.Range(6, 5).Select(n =>
            InlineKeyboardButton.WithCallbackData(n.ToString(), $"game:guess:{sessionId}:{n}")).ToArray();

        await bot.SendMessage(
            chatId,
            $"🎯 <b>{DisplayName}</b>\n\nPick a number <b>1–10</b>.\nSession <code>{sessionId}</code>",
            parseMode: ParseMode.Html,
            replyMarkup: new InlineKeyboardMarkup([row1, row2]),
            cancellationToken: ct);
    }

    public async Task OnCallbackAsync(ITelegramBotClient bot, CallbackQuery query, string payload, CancellationToken ct)
    {
        var parts = payload.Split(':', 2);
        if (parts.Length < 2 || query.From is null || query.Message is null) return;
        var session = await _store.GetAsync(parts[0], ct);
        if (session is null || !session.Active)
        {
            await bot.AnswerCallbackQuery(query.Id, "Game ended", cancellationToken: ct);
            return;
        }

        var guess = parts[1];
        var secret = session.State.GetValueOrDefault("secret", "");
        var tries = int.Parse(session.State.GetValueOrDefault("tries", "0")) + 1;
        session.State["tries"] = tries.ToString();

        if (guess == secret)
        {
            session.Active = false;
            session.Scores[query.From.Id] = session.Scores.GetValueOrDefault(query.From.Id) + 1;
            await _store.SaveAsync(session, ct);
            try
            {
                await bot.EditMessageText(
                    query.Message.Chat.Id, query.Message.MessageId,
                    $"🎯 <b>{DisplayName}</b>\n\n✅ Correct! It was <b>{secret}</b> in {tries} try/tries.",
                    parseMode: ParseMode.Html, cancellationToken: ct);
            }
            catch { }
            await bot.AnswerCallbackQuery(query.Id, "Correct!", cancellationToken: ct);
        }
        else
        {
            await _store.SaveAsync(session, ct);
            await bot.AnswerCallbackQuery(query.Id, "Nope — try again", showAlert: true, cancellationToken: ct);
        }
    }
}

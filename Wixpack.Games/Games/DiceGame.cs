using System.Security.Cryptography;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Wixpack.Games.Core;

namespace Wixpack.Games.Games;

public sealed class DiceGame : IGame
{
    private readonly IGameSessionStore _store;
    public DiceGame(IGameSessionStore store) => _store = store;

    public string Id => "dice";
    public string DisplayName => "Dice Duel";
    public string Description => "Roll a virtual d6 — highest wins.";

    public async Task StartAsync(ITelegramBotClient bot, long chatId, long starterUserId, CancellationToken ct)
    {
        var sessionId = Convert.ToHexString(RandomNumberGenerator.GetBytes(6)).ToLowerInvariant();
        var session = new GameSession
        {
            SessionId = sessionId,
            GameId = Id,
            ChatId = chatId,
            HostUserId = starterUserId
        };
        await _store.SaveAsync(session, ct);

        await bot.SendMessage(
            chatId,
            $"🎲 <b>{DisplayName}</b>\n\nTap roll. First two players settle the duel.\n<code>{sessionId}</code>",
            parseMode: ParseMode.Html,
            replyMarkup: new InlineKeyboardMarkup([[
                InlineKeyboardButton.WithCallbackData("🎲 Roll", $"game:dice:{sessionId}:roll")
            ]]),
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

        var key = $"roll:{query.From.Id}";
        if (session.State.ContainsKey(key))
        {
            await bot.AnswerCallbackQuery(query.Id, "Already rolled", showAlert: true, cancellationToken: ct);
            return;
        }

        var roll = Random.Shared.Next(1, 7);
        session.State[key] = roll.ToString();
        await _store.SaveAsync(session, ct);

        var rolls = session.State.Where(kv => kv.Key.StartsWith("roll:")).ToList();
        if (rolls.Count >= 2)
        {
            var a = rolls[0];
            var b = rolls[1];
            var va = int.Parse(a.Value);
            var vb = int.Parse(b.Value);
            session.Active = false;
            await _store.SaveAsync(session, ct);
            var outcome = va == vb ? "🤝 Draw!" : va > vb ? $"🏆 Roll {va} beats {vb}" : $"🏆 Roll {vb} beats {va}";
            try
            {
                await bot.EditMessageText(
                    query.Message.Chat.Id, query.Message.MessageId,
                    $"🎲 <b>{DisplayName}</b>\n\n{outcome}",
                    parseMode: ParseMode.Html, cancellationToken: ct);
            }
            catch { }
            await bot.AnswerCallbackQuery(query.Id, $"You rolled {roll}", cancellationToken: ct);
        }
        else
        {
            await bot.AnswerCallbackQuery(query.Id, $"You rolled {roll} — waiting…", cancellationToken: ct);
        }
    }
}

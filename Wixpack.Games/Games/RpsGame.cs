using System.Security.Cryptography;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Wixpack.Games.Core;

namespace Wixpack.Games.Games;

/// <summary>
/// Rock-Paper-Scissors multiplayer via inline buttons in groups.
/// </summary>
public sealed class RpsGame : IGame
{
    private readonly IGameSessionStore _store;

    public RpsGame(IGameSessionStore store) => _store = store;

    public string Id => "rps";
    public string DisplayName => "Rock Paper Scissors";
    public string Description => "Challenge the chat — pick rock, paper, or scissors.";

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

        var markup = new InlineKeyboardMarkup([
            [
                InlineKeyboardButton.WithCallbackData("🪨 Rock", $"game:rps:{sessionId}:rock"),
                InlineKeyboardButton.WithCallbackData("📄 Paper", $"game:rps:{sessionId}:paper"),
                InlineKeyboardButton.WithCallbackData("✂ Scissors", $"game:rps:{sessionId}:scissors")
            ]
        ]);

        await bot.SendMessage(
            chatId,
            $"🎮 <b>{DisplayName}</b>\n\nRound open! Everyone pick one.\nSession <code>{sessionId}</code>",
            parseMode: ParseMode.Html,
            replyMarkup: markup,
            cancellationToken: ct);
    }

    public async Task OnCallbackAsync(ITelegramBotClient bot, CallbackQuery query, string payload, CancellationToken ct)
    {
        var parts = payload.Split(':', 2);
        if (parts.Length < 2 || query.From is null || query.Message is null) return;

        var sessionId = parts[0];
        var choice = parts[1];
        var session = await _store.GetAsync(sessionId, ct);
        if (session is null || !session.Active)
        {
            await bot.AnswerCallbackQuery(query.Id, "Game ended", cancellationToken: ct);
            return;
        }

        var key = $"pick:{query.From.Id}";
        if (session.State.ContainsKey(key))
        {
            await bot.AnswerCallbackQuery(query.Id, "You already picked", showAlert: true, cancellationToken: ct);
            return;
        }

        session.State[key] = choice;
        await _store.SaveAsync(session, ct);

        var picks = session.State.Where(kv => kv.Key.StartsWith("pick:")).ToList();
        if (picks.Count >= 2)
        {
            var p1 = picks[0];
            var p2 = picks[1];
            var u1 = long.Parse(p1.Key["pick:".Length..]);
            var u2 = long.Parse(p2.Key["pick:".Length..]);
            var result = Resolve(p1.Value, p2.Value);

            session.Active = false;
            if (result == 1) session.Scores[u1] = session.Scores.GetValueOrDefault(u1) + 1;
            if (result == 2) session.Scores[u2] = session.Scores.GetValueOrDefault(u2) + 1;
            await _store.SaveAsync(session, ct);

            var text = result switch
            {
                0 => $"🤝 Draw! Both chose <b>{p1.Value}</b>.",
                1 => $"🏆 Player wins with <b>{p1.Value}</b> vs {p2.Value}",
                _ => $"🏆 Player wins with <b>{p2.Value}</b> vs {p1.Value}"
            };

            try
            {
                await bot.EditMessageText(
                    query.Message.Chat.Id,
                    query.Message.MessageId,
                    $"🎮 <b>{DisplayName}</b>\n\n{text}",
                    parseMode: ParseMode.Html,
                    cancellationToken: ct);
            }
            catch { }

            await bot.AnswerCallbackQuery(query.Id, "Round resolved", cancellationToken: ct);
        }
        else
        {
            await bot.AnswerCallbackQuery(query.Id, $"Locked in: {choice}", cancellationToken: ct);
        }
    }

    private static int Resolve(string a, string b)
    {
        if (a == b) return 0;
        if ((a == "rock" && b == "scissors") || (a == "paper" && b == "rock") || (a == "scissors" && b == "paper"))
            return 1;
        return 2;
    }
}

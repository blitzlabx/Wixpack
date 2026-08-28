using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Wixpack.Games.Core;

namespace Wixpack.Telegram.Commands;

public sealed class GameCommand : ICommandHandler
{
    private readonly GameRegistry _games;

    public GameCommand(GameRegistry games) => _games = games;

    public string Command => "game";
    public string Description => "Start a group game (e.g. /game rps)";

    public async Task HandleAsync(ITelegramBotClient bot, Message message, string? args, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(args))
        {
            var list = string.Join("\n", _games.All.Select(g => $"• <code>{g.Id}</code> — {g.DisplayName}"));
            await bot.SendMessage(
                message.Chat.Id,
                $"🎮 <b>Games</b>\n\n{list}\n\nUsage: <code>/game rps</code>",
                parseMode: ParseMode.Html,
                cancellationToken: ct);
            return;
        }

        var id = args.Trim().Split(' ')[0];
        var game = _games.Get(id);
        if (game is null)
        {
            await bot.SendMessage(message.Chat.Id, $"Unknown game: {id}. Try /game", cancellationToken: ct);
            return;
        }

        var userId = message.From?.Id ?? 0;
        await game.StartAsync(bot, message.Chat.Id, userId, ct);
    }
}

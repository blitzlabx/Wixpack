using Telegram.Bot;
using Telegram.Bot.Types;

namespace Wixpack.Telegram.Commands;

public interface ICommandHandler
{
    string Command { get; }

    string Description { get; }

    bool AdminOnly => false;

    Task HandleAsync(ITelegramBotClient bot, Message message, string? args, CancellationToken ct);
}

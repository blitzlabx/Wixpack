using Telegram.Bot;
using Telegram.Bot.Types;

namespace Wixpack.Telegram.Commands;

public interface ICommandHandler
{
    /// <summary>Command name without leading slash, e.g. "start".</summary>
    string Command { get; }

    /// <summary>Short description for BotFather /help.</summary>
    string Description { get; }

    /// <summary>Whether this command is only for configured admins.</summary>
    bool AdminOnly => false;

    Task HandleAsync(ITelegramBotClient bot, Message message, string? args, CancellationToken ct);
}

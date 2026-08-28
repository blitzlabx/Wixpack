using Telegram.Bot;
using Telegram.Bot.Types;

namespace Wixpack.Games.Core;

public interface IGame
{
    string Id { get; }
    string DisplayName { get; }
    string Description { get; }

    Task StartAsync(ITelegramBotClient bot, long chatId, long starterUserId, CancellationToken ct);
    Task OnCallbackAsync(ITelegramBotClient bot, CallbackQuery query, string payload, CancellationToken ct);
}

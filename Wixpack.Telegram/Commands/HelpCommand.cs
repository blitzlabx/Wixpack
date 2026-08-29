using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Wixpack.Core.Branding;

namespace Wixpack.Telegram.Commands;

public sealed class HelpCommand : ICommandHandler
{
    public string Command => "help";
    public string Description => "Show available commands";

    public async Task HandleAsync(ITelegramBotClient bot, Message message, string? args, CancellationToken ct)
    {
        var text =
            $"<b>{WixpackBranding.FullProductName}</b>\n\n" +
            "/start — Main menu\n" +
            "/help — This list\n" +
            "/game — List games\n" +
            "/game rps|guess|dice — Start a game\n" +
            "/dl &lt;url&gt; — Download media\n\n" +
            $"@{WixpackBranding.SocialHandle}";

        await bot.SendMessage(message.Chat.Id, text, parseMode: ParseMode.Html, cancellationToken: ct);
    }
}

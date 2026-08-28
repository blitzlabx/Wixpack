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
            $"<b>{WixpackBranding.FullProductName}</b> commands\n\n" +
            "/start — Open main menu\n" +
            "/help — This list\n" +
            "/game — List games\n" +
            "/game rps — Start Rock Paper Scissors\n" +
            "/dl &lt;url&gt; — Download media from a link\n\n" +
            "In private chat you can also send a URL alone.\n" +
            $"@{WixpackBranding.SocialHandle}";

        await bot.SendMessage(
            chatId: message.Chat.Id,
            text: text,
            parseMode: ParseMode.Html,
            cancellationToken: ct);
    }
}

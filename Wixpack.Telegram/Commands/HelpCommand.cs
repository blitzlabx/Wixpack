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
            "/start — Open Wixpack main menu\n" +
            "/help — Show available commands\n\n" +
            "More commands arrive as modules load.\n" +
            $"@{WixpackBranding.SocialHandle}";

        await bot.SendMessage(
            chatId: message.Chat.Id,
            text: text,
            parseMode: ParseMode.Html,
            cancellationToken: ct);
    }
}

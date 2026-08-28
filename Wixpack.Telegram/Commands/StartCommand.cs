using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Wixpack.Core.Branding;
using Wixpack.Telegram.Keyboards;

namespace Wixpack.Telegram.Commands;

public sealed class StartCommand : ICommandHandler
{
    public string Command => "start";
    public string Description => "Open Wixpack main menu";

    public async Task HandleAsync(ITelegramBotClient bot, Message message, string? args, CancellationToken ct)
    {
        var name = message.From?.FirstName ?? "there";
        var text =
            $"Hey <b>{Escape(name)}</b> 👋\n\n" +
            $"Welcome to <b>{WixpackBranding.FullProductName}</b>\n" +
            $"Creator: {WixpackBranding.Creator} · @{WixpackBranding.SocialHandle}\n\n" +
            "Pick a feature below:";

        await bot.SendMessage(
            chatId: message.Chat.Id,
            text: text,
            parseMode: ParseMode.Html,
            replyMarkup: WixpackKeyboards.MainMenu(),
            cancellationToken: ct);
    }

    private static string Escape(string s) =>
        s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
}

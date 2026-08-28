using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Wixpack.Downloader.Services;
using Wixpack.Telegram.Keyboards;

namespace Wixpack.Telegram.Commands;

public sealed class DlCommand : ICommandHandler
{
    private readonly PrexzyDownloaderClient _downloader;

    public DlCommand(PrexzyDownloaderClient downloader) => _downloader = downloader;

    public string Command => "dl";
    public string Description => "Download media — /dl <url>";

    public async Task HandleAsync(ITelegramBotClient bot, Message message, string? args, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(args) || !Uri.TryCreate(args.Trim(), UriKind.Absolute, out _))
        {
            await bot.SendMessage(
                message.Chat.Id,
                "📥 <b>Downloader</b>\n\nSend:\n<code>/dl https://...</code>\n\n" +
                "Supports YouTube, TikTok, Instagram, X/Twitter, Facebook, Spotify, SoundCloud & more.",
                parseMode: ParseMode.Html,
                cancellationToken: ct);
            return;
        }

        var url = args.Trim();
        var status = await bot.SendMessage(
            message.Chat.Id,
            "⏳ Resolving media…",
            cancellationToken: ct);

        var result = await _downloader.ResolveAsync(url, ct);
        if (!result.Success)
        {
            await bot.EditMessageText(
                message.Chat.Id,
                status.MessageId,
                $"❌ {result.Error ?? "Download failed"}",
                cancellationToken: ct);
            return;
        }

        var root = result.Value;
        var title = TryGetString(root, "info", "title")
                    ?? TryGetString(root, "title")
                    ?? "Media";
        var downloadUrl = TryGetString(root, "download_url")
                          ?? TryGetString(root, "url")
                          ?? TryGetNestedDownload(root);
        var quality = TryGetString(root, "quality");
        var ext = TryGetString(root, "ext");

        var text =
            $"✅ <b>{Escape(title)}</b>\n" +
            (quality is not null ? $"Quality: <code>{Escape(quality)}</code>\n" : "") +
            (ext is not null ? $"Format: <code>{Escape(ext)}</code>\n" : "") +
            "\nTap the button below to open the file.";

        InlineKeyboardMarkup? markup = null;
        if (!string.IsNullOrWhiteSpace(downloadUrl) && Uri.TryCreate(downloadUrl, UriKind.Absolute, out _))
        {
            markup = new InlineKeyboardMarkup([
                [ButtonStyles.Url("⬇️ Open download", downloadUrl, ButtonStyles.Success)],
                [ButtonStyles.Callback("« Menu", "menu:main", ButtonStyles.Primary)]
            ]);
        }

        await bot.EditMessageText(
            message.Chat.Id,
            status.MessageId,
            text,
            parseMode: ParseMode.Html,
            replyMarkup: markup,
            cancellationToken: ct);
    }

    private static string? TryGetString(JsonElement root, string prop)
    {
        if (root.TryGetProperty(prop, out var el) && el.ValueKind == JsonValueKind.String)
            return el.GetString();
        return null;
    }

    private static string? TryGetString(JsonElement root, string parent, string child)
    {
        if (root.TryGetProperty(parent, out var p) && p.ValueKind == JsonValueKind.Object)
            return TryGetString(p, child);
        return null;
    }

    private static string? TryGetNestedDownload(JsonElement root)
    {
        if (root.TryGetProperty("result", out var r))
        {
            var u = TryGetString(r, "download_url") ?? TryGetString(r, "url");
            if (u is not null) return u;
        }
        if (root.TryGetProperty("data", out var d))
        {
            var u = TryGetString(d, "download_url") ?? TryGetString(d, "url");
            if (u is not null) return u;
        }
        return null;
    }

    private static string Escape(string s) =>
        s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
}

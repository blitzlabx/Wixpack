using Telegram.Bot.Types.ReplyMarkups;
using Wixpack.Core.Branding;

namespace Wixpack.Telegram.Keyboards;

/// <summary>
/// Shared keyboards for Wixpack menus. Uses free colored styles (primary/danger/success)
/// and default neutral style. No premium-only custom emoji icons.
/// </summary>
public static class WixpackKeyboards
{
    public static InlineKeyboardMarkup MainMenu()
    {
        return new InlineKeyboardMarkup([
            [
                ButtonStyles.Callback("🛠 Dev Tools", "menu:devtools", ButtonStyles.Primary),
                ButtonStyles.Callback("🎮 Games", "menu:games", ButtonStyles.Success)
            ],
            [
                ButtonStyles.Callback("📥 Downloader", "menu:downloader", ButtonStyles.Primary),
                ButtonStyles.Callback("🔐 Floket Verify", "menu:floket", ButtonStyles.Primary)
            ],
            [
                ButtonStyles.Callback("⚙️ Settings", "menu:settings"),
                ButtonStyles.Callback("ℹ️ About", "menu:about")
            ],
            [
                ButtonStyles.Callback("❌ Close", "menu:close", ButtonStyles.Danger)
            ]
        ]);
    }

    public static InlineKeyboardMarkup ConfirmCancel(string confirmCallback, string cancelCallback = "menu:close")
    {
        return new InlineKeyboardMarkup([
            [
                ButtonStyles.Callback("✅ Confirm", confirmCallback, ButtonStyles.Success),
                ButtonStyles.Callback("❌ Cancel", cancelCallback, ButtonStyles.Danger)
            ]
        ]);
    }

    public static InlineKeyboardMarkup About()
    {
        return new InlineKeyboardMarkup([
            [
                ButtonStyles.Callback("« Back", "menu:main", ButtonStyles.Primary)
            ]
        ]);
    }

    public static string AboutText() =>
        $"<b>{WixpackBranding.FullProductName}</b>\n" +
        $"Creator: <b>{WixpackBranding.Creator}</b>\n" +
        $"Handle: @{WixpackBranding.SocialHandle}\n\n" +
        "Modular toolkit: Telegram, Floket security, games, downloader, developer tools, API & more.";
}

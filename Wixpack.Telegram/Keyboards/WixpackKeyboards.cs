using Telegram.Bot.Types.ReplyMarkups;
using Wixpack.Core.Branding;

namespace Wixpack.Telegram.Keyboards;

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
                ButtonStyles.Callback("🔐 Floket", "menu:floket", ButtonStyles.Primary)
            ],
            [
                ButtonStyles.Callback("🎲 Extras", "menu:extras", ButtonStyles.Success),
                ButtonStyles.Callback("⚙️ Settings", "menu:settings")
            ],
            [
                ButtonStyles.Callback("ℹ️ About", "menu:about"),
                ButtonStyles.Callback("❌ Close", "menu:close", ButtonStyles.Danger)
            ]
        ]);
    }

    public static InlineKeyboardMarkup GamesMenu()
    {
        return new InlineKeyboardMarkup([
            [
                ButtonStyles.Callback("🪨📄✂ RPS", "game:start:rps", ButtonStyles.Success),
                ButtonStyles.Callback("🎯 Guess", "game:start:guess", ButtonStyles.Primary)
            ],
            [
                ButtonStyles.Callback("🎲 Dice Duel", "game:start:dice", ButtonStyles.Success)
            ],
            [
                ButtonStyles.Callback("« Back", "menu:main", ButtonStyles.Primary)
            ]
        ]);
    }

    public static InlineKeyboardMarkup DownloaderMenu()
    {
        return new InlineKeyboardMarkup([
            [ButtonStyles.Callback("« Back", "menu:main", ButtonStyles.Primary)]
        ]);
    }

    public static InlineKeyboardMarkup DevToolsMenu()
    {
        return new InlineKeyboardMarkup([
            [
                ButtonStyles.Callback("🆔 UUID", "tool:uuid", ButtonStyles.Primary),
                ButtonStyles.Callback("🆔×5 UUIDs", "tool:uuid5", ButtonStyles.Primary)
            ],
            [
                ButtonStyles.Callback("⏱ Timestamp", "tool:timestamp", ButtonStyles.Success),
                ButtonStyles.Callback("🔑 Password", "tool:password", ButtonStyles.Danger)
            ],
            [
                ButtonStyles.Callback("🎲 Coin", "tool:coinflip", ButtonStyles.Success),
                ButtonStyles.Callback("📊 Stats demo", "tool:stats", ButtonStyles.Primary)
            ],
            [
                ButtonStyles.Callback("📝 Lorem", "tool:lorem"),
                ButtonStyles.Callback("🎨 #hex→rgb", "tool:color")
            ],
            [
                ButtonStyles.Callback("« Back", "menu:main", ButtonStyles.Primary)
            ]
        ]);
    }

    public static InlineKeyboardMarkup ExtrasMenu()
    {
        return new InlineKeyboardMarkup([
            [
                ButtonStyles.Callback("🐱 Random cat", "extra:cat", ButtonStyles.Success),
                ButtonStyles.Callback("🐶 Random dog", "extra:dog", ButtonStyles.Primary)
            ],
            [
                ButtonStyles.Callback("🧠 Quiz", "extra:quiz", ButtonStyles.Success)
            ],
            [
                ButtonStyles.Callback("« Back", "menu:main", ButtonStyles.Primary)
            ]
        ]);
    }

    public static InlineKeyboardMarkup SettingsMenu(string? donationUrl)
    {
        var rows = new List<InlineKeyboardButton[]>();
        if (!string.IsNullOrWhiteSpace(donationUrl) && Uri.TryCreate(donationUrl, UriKind.Absolute, out _))
            rows.Add([ButtonStyles.Url("💚 Donate", donationUrl, ButtonStyles.Success)]);
        rows.Add([ButtonStyles.Callback("« Back", "menu:main", ButtonStyles.Primary)]);
        return new InlineKeyboardMarkup(rows);
    }

    public static InlineKeyboardMarkup BackOnly() =>
        new([[ButtonStyles.Callback("« Back", "menu:main", ButtonStyles.Primary)]]);

    public static InlineKeyboardMarkup About() => BackOnly();

    public static string AboutText() =>
        $"<b>{WixpackBranding.FullProductName}</b>\n" +
        $"Creator: <b>{WixpackBranding.Creator}</b>\n" +
        $"Telegram: @{WixpackBranding.SocialHandle}\n\n" +
        "Telegram bot · Floket security · games · media tools · developer utilities · HTTP API.";
}

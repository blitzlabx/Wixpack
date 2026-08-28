using Telegram.Bot.Types.ReplyMarkups;

namespace Wixpack.Telegram.Keyboards;

/// <summary>
/// Helpers for Telegram button styles (Bot API 9.4+).
/// Colors are free for all bots. Custom emoji icons require Premium — not used by default.
/// </summary>
public static class ButtonStyles
{
    /// <summary>Blue — primary / main actions.</summary>
    public static KeyboardButtonStyle Primary => KeyboardButtonStyle.Primary;

    /// <summary>Red — destructive / cancel / ban.</summary>
    public static KeyboardButtonStyle Danger => KeyboardButtonStyle.Danger;

    /// <summary>Green — confirm / success / accept.</summary>
    public static KeyboardButtonStyle Success => KeyboardButtonStyle.Success;

    /// <summary>
    /// Create a styled callback button.
    /// Omit style for default transparent/neutral look.
    /// </summary>
    public static InlineKeyboardButton Callback(
        string text,
        string callbackData,
        KeyboardButtonStyle? style = null)
    {
        var btn = InlineKeyboardButton.WithCallbackData(text, callbackData);
        if (style.HasValue)
            btn.Style = style.Value;
        return btn;
    }

    public static InlineKeyboardButton Url(
        string text,
        string url,
        KeyboardButtonStyle? style = null)
    {
        var btn = InlineKeyboardButton.WithUrl(text, url);
        if (style.HasValue)
            btn.Style = style.Value;
        return btn;
    }
}

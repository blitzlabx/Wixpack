using Telegram.Bot.Types.ReplyMarkups;

namespace Wixpack.Telegram.Keyboards;

public static class ButtonStyles
{
    public static KeyboardButtonStyle Primary => KeyboardButtonStyle.Primary;

    public static KeyboardButtonStyle Danger => KeyboardButtonStyle.Danger;

    public static KeyboardButtonStyle Success => KeyboardButtonStyle.Success;

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

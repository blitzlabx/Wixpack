using Wixpack.Floket.Models;

namespace Wixpack.Floket.Telegram;

public static class FloketTelegramMessages
{
    public const string PoweredBy = "🔐 Powered by Floket";

    public static string ChallengeMessage(VerificationSession session)
    {
        var name = string.IsNullOrWhiteSpace(session.FirstName) ? "there" : session.FirstName;
        return
            $"<b>Floket Human Verification</b>\n\n" +
            $"Hey {Escape(name)}, this group is protected.\n" +
            $"Complete the challenge below to unlock access.\n\n" +
            $"❓ <b>{Escape(session.ChallengePrompt)}</b>\n\n" +
            $"⏱ Expires in ~{(int)(session.ExpiresAt - session.CreatedAt).TotalSeconds}s · " +
            $"Attempts: {session.MaxAttempts}\n\n" +
            $"<i>{PoweredBy}</i>";
    }

    public static string SuccessMessage() =>
        "✅ <b>Verified by Floket</b>\n\nYou now have access to this group.\n\n" +
        $"<i>{PoweredBy}</i>";

    public static string FailureMessage(string reason) =>
        $"❌ <b>Floket verification failed</b>\n\n{Escape(reason)}\n\n" +
        $"<i>{PoweredBy}</i>";

    public static string ExpiredMessage() =>
        $"⌛ <b>Floket session expired</b>\n\nRejoin the group to start a new verification.\n\n" +
        $"<i>{PoweredBy}</i>";

    private static string Escape(string s) =>
        s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
}

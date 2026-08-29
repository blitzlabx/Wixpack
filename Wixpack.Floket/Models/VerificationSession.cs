namespace Wixpack.Floket.Models;

public enum VerificationStatus
{
    Pending,
    Verified,
    Failed,
    Expired,
    Blocked
}

public sealed class VerificationSession
{
    public required string SessionId { get; init; }
    public required long UserId { get; init; }
    public required long ChatId { get; init; }
    public required string ChallengeToken { get; init; }
    public required string CorrectAnswer { get; init; }
    public required string ChallengePrompt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset ExpiresAt { get; init; }
    public int Attempts { get; set; }
    public int MaxAttempts { get; init; }
    public VerificationStatus Status { get; set; } = VerificationStatus.Pending;
    public string? Username { get; init; }
    public string? FirstName { get; init; }
}

public sealed class GroupFloketConfig
{
    public long ChatId { get; set; }
    public bool Enabled { get; set; } = true;
    public int TimeoutSeconds { get; set; } = 120;
    public int MaxAttempts { get; set; } = 3;
    public bool RestrictUntilVerified { get; set; } = true;
    public bool KickOnFailure { get; set; } = true;
}

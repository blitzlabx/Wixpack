using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wixpack.Core.Configuration;
using Wixpack.Core.Models;
using Wixpack.Core.Services;
using Wixpack.Floket.Challenges;
using Wixpack.Floket.Models;

namespace Wixpack.Floket.Services;

public sealed class FloketVerificationService
{
    private readonly IFloketVerificationStore _store;
    private readonly IChallengeGenerator _challenges;
    private readonly IClock _clock;
    private readonly FloketSettings _defaults;
    private readonly ILogger<FloketVerificationService> _logger;

    public FloketVerificationService(
        IFloketVerificationStore store,
        IChallengeGenerator challenges,
        IClock clock,
        IOptions<WixpackSettings> settings,
        ILogger<FloketVerificationService> logger)
    {
        _store = store;
        _challenges = challenges;
        _clock = clock;
        _defaults = settings.Value.Floket;
        _logger = logger;
    }

    public async Task<Result<VerificationSession>> StartVerificationAsync(
        long userId,
        long chatId,
        string? username,
        string? firstName,
        CancellationToken ct = default)
    {
        if (await _store.IsUserVerifiedAsync(userId, chatId, ct))
            return Result<VerificationSession>.Fail("Already verified");

        var existing = await _store.GetPendingByUserChatAsync(userId, chatId, ct);
        if (existing is not null && existing.ExpiresAt > _clock.UtcNow)
            return Result<VerificationSession>.Ok(existing);

        var config = await ResolveConfigAsync(chatId, ct);
        var (prompt, answer) = _challenges.Generate();
        var now = _clock.UtcNow;
        var session = new VerificationSession
        {
            SessionId = GenerateId(),
            UserId = userId,
            ChatId = chatId,
            ChallengeToken = GenerateToken(),
            CorrectAnswer = answer,
            ChallengePrompt = prompt,
            CreatedAt = now,
            ExpiresAt = now.AddSeconds(config.TimeoutSeconds),
            MaxAttempts = config.MaxAttempts,
            Username = username,
            FirstName = firstName
        };

        await _store.SaveSessionAsync(session, ct);
        _logger.LogInformation(
            "Floket session {SessionId} started for user {UserId} in chat {ChatId}",
            session.SessionId, userId, chatId);

        return Result<VerificationSession>.Ok(session);
    }

    public async Task<Result> SubmitAnswerAsync(string sessionId, string answer, CancellationToken ct = default)
    {
        var session = await _store.GetSessionAsync(sessionId, ct);
        if (session is null)
            return Result.Fail("Invalid or unknown verification session");

        if (session.Status != VerificationStatus.Pending)
            return Result.Fail($"Session is {session.Status}");

        if (session.ExpiresAt < _clock.UtcNow)
        {
            session.Status = VerificationStatus.Expired;
            await _store.UpdateSessionAsync(session, ct);
            return Result.Fail("Verification expired. Rejoin the group to try again.");
        }

        session.Attempts++;

        if (string.Equals(answer.Trim(), session.CorrectAnswer, StringComparison.OrdinalIgnoreCase))
        {
            session.Status = VerificationStatus.Verified;
            await _store.UpdateSessionAsync(session, ct);
            await _store.MarkVerifiedAsync(session.UserId, session.ChatId, ct);
            _logger.LogInformation("Floket verified user {UserId} in chat {ChatId}", session.UserId, session.ChatId);
            return Result.Ok();
        }

        if (session.Attempts >= session.MaxAttempts)
        {
            session.Status = VerificationStatus.Failed;
            await _store.UpdateSessionAsync(session, ct);
            _logger.LogWarning(
                "Floket failed user {UserId} in chat {ChatId} after {Attempts} attempts",
                session.UserId, session.ChatId, session.Attempts);
            return Result.Fail("Too many failed attempts. Access denied.");
        }

        await _store.UpdateSessionAsync(session, ct);
        var left = session.MaxAttempts - session.Attempts;
        return Result.Fail($"Incorrect. {left} attempt(s) remaining.");
    }

    public Task<bool> IsVerifiedAsync(long userId, long chatId, CancellationToken ct = default) =>
        _store.IsUserVerifiedAsync(userId, chatId, ct);

    public async Task<GroupFloketConfig> ResolveConfigAsync(long chatId, CancellationToken ct = default)
    {
        var existing = await _store.GetGroupConfigAsync(chatId, ct);
        if (existing is not null) return existing;

        return new GroupFloketConfig
        {
            ChatId = chatId,
            Enabled = _defaults.EnabledByDefault,
            TimeoutSeconds = _defaults.VerificationTimeoutSeconds,
            MaxAttempts = _defaults.MaxAttempts,
            RestrictUntilVerified = _defaults.RestrictUntilVerified,
            KickOnFailure = true
        };
    }

    public Task EnableGroupAsync(long chatId, bool enabled, CancellationToken ct = default)
    {
        return _store.SetGroupConfigAsync(new GroupFloketConfig
        {
            ChatId = chatId,
            Enabled = enabled,
            TimeoutSeconds = _defaults.VerificationTimeoutSeconds,
            MaxAttempts = _defaults.MaxAttempts,
            RestrictUntilVerified = _defaults.RestrictUntilVerified
        }, ct);
    }

    private static string GenerateId() => Convert.ToHexString(RandomNumberGenerator.GetBytes(8)).ToLowerInvariant();
    private static string GenerateToken() => Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant();
}

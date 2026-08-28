using Wixpack.Floket.Models;

namespace Wixpack.Floket.Services;

public interface IFloketVerificationStore
{
    Task SaveSessionAsync(VerificationSession session, CancellationToken ct = default);
    Task<VerificationSession?> GetSessionAsync(string sessionId, CancellationToken ct = default);
    Task<VerificationSession?> GetPendingByUserChatAsync(long userId, long chatId, CancellationToken ct = default);
    Task UpdateSessionAsync(VerificationSession session, CancellationToken ct = default);
    Task<bool> IsUserVerifiedAsync(long userId, long chatId, CancellationToken ct = default);
    Task MarkVerifiedAsync(long userId, long chatId, CancellationToken ct = default);
    Task<GroupFloketConfig?> GetGroupConfigAsync(long chatId, CancellationToken ct = default);
    Task SetGroupConfigAsync(GroupFloketConfig config, CancellationToken ct = default);
    Task CleanupExpiredAsync(CancellationToken ct = default);
}

/// <summary>
/// In-memory store suitable for single-instance deploy (e.g. Render free).
/// Replace with Redis/DB for multi-instance production.
/// </summary>
public sealed class InMemoryFloketStore : IFloketVerificationStore
{
    private readonly Dictionary<string, VerificationSession> _sessions = new();
    private readonly HashSet<(long UserId, long ChatId)> _verified = new();
    private readonly Dictionary<long, GroupFloketConfig> _groups = new();
    private readonly object _lock = new();

    public Task SaveSessionAsync(VerificationSession session, CancellationToken ct = default)
    {
        lock (_lock) _sessions[session.SessionId] = session;
        return Task.CompletedTask;
    }

    public Task<VerificationSession?> GetSessionAsync(string sessionId, CancellationToken ct = default)
    {
        lock (_lock)
            return Task.FromResult(_sessions.TryGetValue(sessionId, out var s) ? s : null);
    }

    public Task<VerificationSession?> GetPendingByUserChatAsync(long userId, long chatId, CancellationToken ct = default)
    {
        lock (_lock)
        {
            var s = _sessions.Values.FirstOrDefault(x =>
                x.UserId == userId && x.ChatId == chatId && x.Status == VerificationStatus.Pending);
            return Task.FromResult(s);
        }
    }

    public Task UpdateSessionAsync(VerificationSession session, CancellationToken ct = default)
    {
        lock (_lock) _sessions[session.SessionId] = session;
        return Task.CompletedTask;
    }

    public Task<bool> IsUserVerifiedAsync(long userId, long chatId, CancellationToken ct = default)
    {
        lock (_lock) return Task.FromResult(_verified.Contains((userId, chatId)));
    }

    public Task MarkVerifiedAsync(long userId, long chatId, CancellationToken ct = default)
    {
        lock (_lock) _verified.Add((userId, chatId));
        return Task.CompletedTask;
    }

    public Task<GroupFloketConfig?> GetGroupConfigAsync(long chatId, CancellationToken ct = default)
    {
        lock (_lock)
            return Task.FromResult(_groups.TryGetValue(chatId, out var c) ? c : null);
    }

    public Task SetGroupConfigAsync(GroupFloketConfig config, CancellationToken ct = default)
    {
        lock (_lock) _groups[config.ChatId] = config;
        return Task.CompletedTask;
    }

    public Task CleanupExpiredAsync(CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        lock (_lock)
        {
            var expired = _sessions.Where(kv =>
                kv.Value.Status == VerificationStatus.Pending && kv.Value.ExpiresAt < now).ToList();
            foreach (var (id, session) in expired)
            {
                session.Status = VerificationStatus.Expired;
                _sessions[id] = session;
            }
        }
        return Task.CompletedTask;
    }
}

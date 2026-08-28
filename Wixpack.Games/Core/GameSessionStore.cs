namespace Wixpack.Games.Core;

public sealed class GameSession
{
    public required string SessionId { get; init; }
    public required string GameId { get; init; }
    public required long ChatId { get; init; }
    public required long HostUserId { get; init; }
    public Dictionary<string, string> State { get; } = new();
    public Dictionary<long, int> Scores { get; } = new();
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public bool Active { get; set; } = true;
}

public interface IGameSessionStore
{
    Task SaveAsync(GameSession session, CancellationToken ct = default);
    Task<GameSession?> GetAsync(string sessionId, CancellationToken ct = default);
    Task<GameSession?> GetActiveInChatAsync(long chatId, CancellationToken ct = default);
}

public sealed class InMemoryGameSessionStore : IGameSessionStore
{
    private readonly Dictionary<string, GameSession> _sessions = new();
    private readonly object _lock = new();

    public Task SaveAsync(GameSession session, CancellationToken ct = default)
    {
        lock (_lock) _sessions[session.SessionId] = session;
        return Task.CompletedTask;
    }

    public Task<GameSession?> GetAsync(string sessionId, CancellationToken ct = default)
    {
        lock (_lock) return Task.FromResult(_sessions.TryGetValue(sessionId, out var s) ? s : null);
    }

    public Task<GameSession?> GetActiveInChatAsync(long chatId, CancellationToken ct = default)
    {
        lock (_lock)
        {
            var s = _sessions.Values.FirstOrDefault(x => x.ChatId == chatId && x.Active);
            return Task.FromResult(s);
        }
    }
}

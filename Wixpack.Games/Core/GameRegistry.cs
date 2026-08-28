namespace Wixpack.Games.Core;

public sealed class GameRegistry
{
    private readonly Dictionary<string, IGame> _games;

    public GameRegistry(IEnumerable<IGame> games)
    {
        _games = games.ToDictionary(g => g.Id, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyCollection<IGame> All => _games.Values;
    public IGame? Get(string id) => _games.TryGetValue(id, out var g) ? g : null;
}

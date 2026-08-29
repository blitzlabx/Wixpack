using Microsoft.Extensions.DependencyInjection;
using Wixpack.Games.Core;
using Wixpack.Games.Games;

namespace Wixpack.Games.DependencyInjection;

public static class GamesServiceCollectionExtensions
{
    public static IServiceCollection AddWixpackGames(this IServiceCollection services)
    {
        services.AddSingleton<IGameSessionStore, InMemoryGameSessionStore>();
        services.AddSingleton<IGame, RpsGame>();
        services.AddSingleton<IGame, NumberGuessGame>();
        services.AddSingleton<IGame, DiceGame>();
        services.AddSingleton<GameRegistry>();
        return services;
    }
}

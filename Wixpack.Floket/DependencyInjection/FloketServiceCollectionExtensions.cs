using Microsoft.Extensions.DependencyInjection;
using Wixpack.Floket.Challenges;
using Wixpack.Floket.Services;

namespace Wixpack.Floket.DependencyInjection;

public static class FloketServiceCollectionExtensions
{
    public static IServiceCollection AddWixpackFloket(this IServiceCollection services)
    {
        services.AddSingleton<IFloketVerificationStore, InMemoryFloketStore>();
        services.AddSingleton<IChallengeGenerator, MathChallengeGenerator>();
        services.AddSingleton<FloketVerificationService>();
        return services;
    }
}

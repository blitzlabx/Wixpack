using Microsoft.Extensions.DependencyInjection;
using Wixpack.Experimental.Features;

namespace Wixpack.Experimental.DependencyInjection;

public static class ExperimentalServiceCollectionExtensions
{
    public static IServiceCollection AddWixpackExperimental(this IServiceCollection services)
    {
        services.AddSingleton<CoinFlipFeature>();
        return services;
    }
}

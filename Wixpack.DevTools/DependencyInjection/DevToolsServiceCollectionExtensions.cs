using Microsoft.Extensions.DependencyInjection;
using Wixpack.DevTools.Services;

namespace Wixpack.DevTools.DependencyInjection;

public static class DevToolsServiceCollectionExtensions
{
    public static IServiceCollection AddWixpackDevTools(this IServiceCollection services)
    {
        services.AddSingleton<DevToolsService>();
        return services;
    }
}

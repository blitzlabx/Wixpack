using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wixpack.Core.Configuration;
using Wixpack.Core.Services;

namespace Wixpack.Core.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWixpackCore(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<WixpackSettings>(configuration);
        services.AddSingleton<IClock, SystemClock>();
        return services;
    }
}

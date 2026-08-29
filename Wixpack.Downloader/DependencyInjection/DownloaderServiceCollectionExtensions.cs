using Microsoft.Extensions.DependencyInjection;
using Wixpack.Downloader.Services;

namespace Wixpack.Downloader.DependencyInjection;

public static class DownloaderServiceCollectionExtensions
{
    public static IServiceCollection AddWixpackDownloader(this IServiceCollection services)
    {
        services.AddHttpClient<PrexzyDownloaderClient>(c =>
        {
            c.BaseAddress = new Uri(PrexzyDownloaderClient.BaseUrl);
            c.Timeout = TimeSpan.FromSeconds(60);
            c.DefaultRequestHeaders.UserAgent.ParseAdd("Wixpack/1.0");
        });
        services.AddHttpClient<MediaExtrasClient>(c =>
        {
            c.BaseAddress = new Uri(MediaExtrasClient.BaseUrl);
            c.Timeout = TimeSpan.FromSeconds(45);
            c.DefaultRequestHeaders.UserAgent.ParseAdd("Wixpack/1.0");
        });
        return services;
    }
}

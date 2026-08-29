using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Wixpack.Core.Models;

namespace Wixpack.Downloader.Services;

public sealed class PrexzyDownloaderClient
{
    public const string BaseUrl = "https://prexzyapis.com";

    private readonly HttpClient _http;
    private readonly ILogger<PrexzyDownloaderClient> _logger;

    public PrexzyDownloaderClient(HttpClient http, ILogger<PrexzyDownloaderClient> logger)
    {
        _http = http;
        _logger = logger;
        if (_http.BaseAddress is null)
            _http.BaseAddress = new Uri(BaseUrl);
    }

    public async Task<Result<JsonElement>> ResolveAsync(string url, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out _))
            return Result<JsonElement>.Fail("A valid absolute URL is required");

        var host = new Uri(url).Host.ToLowerInvariant();
        var path = PickEndpoint(host, url);
        var requestUrl = $"{path}?url={Uri.EscapeDataString(url)}";

        try
        {
            using var resp = await _http.GetAsync(requestUrl, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement.Clone();

            var ok = root.TryGetProperty("status", out var st) && st.ValueKind == JsonValueKind.True;
            if (!ok && root.TryGetProperty("statusCode", out var code) && code.TryGetInt32(out var c) && c == 200)
                ok = true;

            if (!ok)
            {
                var err = root.TryGetProperty("error", out var e) ? e.GetString() : "Download resolve failed";
                return Result<JsonElement>.Fail(err ?? "Download resolve failed");
            }

            return Result<JsonElement>.Ok(root);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Prexzy resolve failed for {Url}", url);
            return Result<JsonElement>.Fail(ex.Message);
        }
    }

    public async Task<Result<JsonElement>> YoutubeInfoAsync(string url, CancellationToken ct = default)
    {
        try
        {
            using var resp = await _http.GetAsync($"/download/ytinfo?url={Uri.EscapeDataString(url)}", ct);
            var body = await resp.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(body);
            return Result<JsonElement>.Ok(doc.RootElement.Clone());
        }
        catch (Exception ex)
        {
            return Result<JsonElement>.Fail(ex.Message);
        }
    }

    private static string PickEndpoint(string host, string url)
    {
        if (host.Contains("youtube") || host.Contains("youtu.be"))
            return "/download/ytmp4";
        if (host.Contains("tiktok") || host.Contains("vm.tiktok"))
            return "/download/tiktokV2";
        if (host.Contains("instagram"))
            return "/download/instagram";
        if (host.Contains("twitter") || host.Contains("x.com"))
            return "/download/twitter";
        if (host.Contains("facebook") || host.Contains("fb.watch"))
            return "/download/facebook";
        if (host.Contains("soundcloud"))
            return "/download/soundcloud";
        if (host.Contains("spotify"))
            return "/download/spotify";
        if (host.Contains("mediafire"))
            return "/download/mediafire";
        if (host.Contains("pinterest"))
            return "/download/pinterestV2";
        // Universal fallback
        return "/download/aiov2";
    }
}

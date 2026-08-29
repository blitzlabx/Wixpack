using System.Text.Json;
using Microsoft.Extensions.Logging;
using Wixpack.Core.Models;

namespace Wixpack.Downloader.Services;

public sealed class MediaExtrasClient
{
    public const string BaseUrl = "https://prexzyapis.com";
    private readonly HttpClient _http;
    private readonly ILogger<MediaExtrasClient> _logger;

    public MediaExtrasClient(HttpClient http, ILogger<MediaExtrasClient> logger)
    {
        _http = http;
        _logger = logger;
        _http.BaseAddress ??= new Uri(BaseUrl);
    }

    public async Task<Result<JsonElement>> GetAsync(string pathAndQuery, CancellationToken ct = default)
    {
        try
        {
            using var resp = await _http.GetAsync(pathAndQuery, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(body);
            return Result<JsonElement>.Ok(doc.RootElement.Clone());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Media extras request failed: {Path}", pathAndQuery);
            return Result<JsonElement>.Fail(ex.Message);
        }
    }

    public Task<Result<JsonElement>> TextStylesAsync(string text, CancellationToken ct = default) =>
        GetAsync($"/tools/allstyles?text={Uri.EscapeDataString(text)}", ct);

    public Task<Result<JsonElement>> ShortenAsync(string url, CancellationToken ct = default) =>
        GetAsync($"/tools/vgd?url={Uri.EscapeDataString(url)}", ct);

    public Task<Result<JsonElement>> QuizCategoriesAsync(CancellationToken ct = default) =>
        GetAsync("/game/quizcategories", ct);

    public Task<Result<JsonElement>> QuizRandomAsync(CancellationToken ct = default) =>
        GetAsync("/game/quizrandom", ct);

    public Task<Result<JsonElement>> RandomCatAsync(CancellationToken ct = default) =>
        GetAsync("/random/cat", ct);

    public Task<Result<JsonElement>> RandomDogAsync(CancellationToken ct = default) =>
        GetAsync("/random/dog", ct);

    public Task<Result<JsonElement>> ScreenshotAsync(string url, CancellationToken ct = default) =>
        GetAsync($"/ssweb/webss?url={Uri.EscapeDataString(url)}", ct);
}

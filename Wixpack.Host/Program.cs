using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Wixpack.Core.Branding;
using Wixpack.Core.Configuration;
using Wixpack.Core.DependencyInjection;
using Wixpack.Core.Logging;
using Wixpack.DevTools.DependencyInjection;
using Wixpack.DevTools.Services;
using Wixpack.Downloader.DependencyInjection;
using Wixpack.Downloader.Services;
using Wixpack.Experimental.DependencyInjection;
using Wixpack.Experimental.Features;
using Wixpack.Floket.DependencyInjection;
using Wixpack.Games.DependencyInjection;
using Wixpack.Telegram.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

var configPath = Path.Combine(AppContext.BaseDirectory, "config", "settings.json");
if (!File.Exists(configPath))
    configPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "config", "settings.json"));
if (File.Exists(configPath))
    builder.Configuration.AddJsonFile(configPath, optional: true, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables(prefix: "WIXPACK_");

var port = Environment.GetEnvironmentVariable("PORT") ?? "5080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var settings = new WixpackSettings();
builder.Configuration.Bind(settings);

Log.Logger = WixpackLog.CreateLogger(settings.Logging, WixpackBranding.ProductName);
builder.Host.UseSerilog();

builder.Services.AddWixpackCore(builder.Configuration);
builder.Services.AddWixpackFloket();
builder.Services.AddWixpackGames();
builder.Services.AddWixpackDevTools();
builder.Services.AddWixpackDownloader();
builder.Services.AddWixpackExperimental();
builder.Services.AddWixpackTelegram(builder.Configuration);

var app = builder.Build();

// ── Keep-alive (Render cron) ───────────────────────────────────────────
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    product = WixpackBranding.FullProductName,
    creator = WixpackBranding.Creator,
    handle = WixpackBranding.SocialHandle,
    utc = DateTimeOffset.UtcNow
}));
app.MapGet("/ping", () => Results.Text("pong"));
app.MapGet("/", () => Results.Ok(new
{
    name = WixpackBranding.FullProductName,
    creator = WixpackBranding.Creator,
    handle = WixpackBranding.SocialHandle,
    health = "/health",
    ping = "/ping",
    api = "/api",
    docs = new
    {
        health = "GET /health",
        download = "POST /api/download { \"url\": \"...\" }",
        tools = "POST /api/tools/{action}",
        experimental = "GET /api/experimental/coin-flip"
    }
}));

// ── Wixpack API ────────────────────────────────────────────────────────
var api = app.MapGroup("/api");

api.MapGet("/", () => Results.Ok(new
{
    product = WixpackBranding.FullProductName,
    version = "0.9.0",
    endpoints = new[]
    {
        "GET  /api",
        "POST /api/download",
        "GET  /api/download/ytinfo?url=",
        "POST /api/tools/json/format",
        "POST /api/tools/json/minify",
        "POST /api/tools/base64/encode",
        "POST /api/tools/base64/decode",
        "POST /api/tools/url/encode",
        "POST /api/tools/url/decode",
        "GET  /api/tools/uuid",
        "POST /api/tools/hash",
        "POST /api/tools/regex",
        "POST /api/tools/jwt",
        "GET  /api/tools/timestamp",
        "POST /api/tools/timestamp",
        "POST /api/tools/qr",
        "GET  /api/experimental/coin-flip"
    }
}));

api.MapPost("/download", async (HttpRequest req, PrexzyDownloaderClient dl) =>
{
    using var doc = await JsonDocument.ParseAsync(req.Body);
    if (!doc.RootElement.TryGetProperty("url", out var urlEl))
        return Results.BadRequest(new { error = "url is required" });
    var url = urlEl.GetString() ?? "";
    var result = await dl.ResolveAsync(url);
    if (!result.Success)
        return Results.BadRequest(new { error = result.Error });
    return Results.Json(result.Value);
});

api.MapGet("/download/ytinfo", async (string url, PrexzyDownloaderClient dl) =>
{
    var result = await dl.YoutubeInfoAsync(url);
    if (!result.Success)
        return Results.BadRequest(new { error = result.Error });
    return Results.Json(result.Value);
});

api.MapPost("/tools/json/format", async (HttpRequest req, DevToolsService tools) =>
{
    var body = await new StreamReader(req.Body).ReadToEndAsync();
    var r = tools.FormatJson(body);
    return r.Success ? Results.Text(r.Value!, "application/json") : Results.BadRequest(new { error = r.Error });
});

api.MapPost("/tools/json/minify", async (HttpRequest req, DevToolsService tools) =>
{
    var body = await new StreamReader(req.Body).ReadToEndAsync();
    var r = tools.MinifyJson(body);
    return r.Success ? Results.Text(r.Value!, "application/json") : Results.BadRequest(new { error = r.Error });
});

api.MapPost("/tools/base64/encode", async (HttpRequest req, DevToolsService tools) =>
{
    var body = await new StreamReader(req.Body).ReadToEndAsync();
    return Results.Ok(new { result = tools.Base64Encode(body) });
});

api.MapPost("/tools/base64/decode", async (HttpRequest req, DevToolsService tools) =>
{
    var body = await new StreamReader(req.Body).ReadToEndAsync();
    var r = tools.Base64Decode(body);
    return r.Success ? Results.Ok(new { result = r.Value }) : Results.BadRequest(new { error = r.Error });
});

api.MapPost("/tools/url/encode", async (HttpRequest req, DevToolsService tools) =>
{
    var body = await new StreamReader(req.Body).ReadToEndAsync();
    return Results.Ok(new { result = tools.UrlEncode(body) });
});

api.MapPost("/tools/url/decode", async (HttpRequest req, DevToolsService tools) =>
{
    var body = await new StreamReader(req.Body).ReadToEndAsync();
    return Results.Ok(new { result = tools.UrlDecode(body) });
});

api.MapGet("/tools/uuid", (DevToolsService tools) => Results.Ok(new { uuid = tools.NewGuid() }));

api.MapPost("/tools/hash", async (HttpRequest req, DevToolsService tools) =>
{
    using var doc = await JsonDocument.ParseAsync(req.Body);
    var text = doc.RootElement.GetProperty("text").GetString() ?? "";
    var algo = doc.RootElement.TryGetProperty("algorithm", out var a) ? a.GetString() ?? "SHA256" : "SHA256";
    return Results.Ok(new { algorithm = algo, hash = tools.Hash(text, algo) });
});

api.MapPost("/tools/regex", async (HttpRequest req, DevToolsService tools) =>
{
    using var doc = await JsonDocument.ParseAsync(req.Body);
    var pattern = doc.RootElement.GetProperty("pattern").GetString() ?? "";
    var input = doc.RootElement.GetProperty("input").GetString() ?? "";
    var r = tools.TestRegex(pattern, input);
    return r.Success ? Results.Ok(r.Value) : Results.BadRequest(new { error = r.Error });
});

api.MapPost("/tools/jwt", async (HttpRequest req, DevToolsService tools) =>
{
    var token = (await new StreamReader(req.Body).ReadToEndAsync()).Trim().Trim('"');
    var r = tools.DecodeJwt(token);
    return r.Success ? Results.Ok(r.Value) : Results.BadRequest(new { error = r.Error });
});

api.MapGet("/tools/timestamp", (DevToolsService tools) => Results.Ok(tools.TimestampNow()));

api.MapPost("/tools/timestamp", async (HttpRequest req, DevToolsService tools) =>
{
    using var doc = await JsonDocument.ParseAsync(req.Body);
    var value = doc.RootElement.GetProperty("value").GetInt64();
    var ms = doc.RootElement.TryGetProperty("milliseconds", out var m) && m.GetBoolean();
    var r = tools.TimestampConvert(value, ms);
    return r.Success ? Results.Ok(r.Value) : Results.BadRequest(new { error = r.Error });
});

api.MapPost("/tools/qr", async (HttpRequest req, DevToolsService tools) =>
{
    var text = (await new StreamReader(req.Body).ReadToEndAsync()).Trim();
    if (string.IsNullOrEmpty(text))
        return Results.BadRequest(new { error = "text body required" });
    var png = tools.GenerateQrPng(text);
    return Results.File(png, "image/png");
});

api.MapGet("/experimental/coin-flip", (CoinFlipFeature flip) =>
    Results.Ok(new { feature = flip.Name, result = flip.Flip(), note = "experimental — isolated from core" }));

Log.Information("{Banner}", WixpackBranding.VersionBanner("0.9.0"));
Log.Information("Listening on port {Port}. Health: /health  API: /api", port);

await app.RunAsync();

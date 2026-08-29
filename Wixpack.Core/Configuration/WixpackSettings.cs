namespace Wixpack.Core.Configuration;

public sealed class WixpackSettings
{
    public const string SectionName = "Wixpack";

    public string LogoUrl { get; set; } = "";

    public string DonationUrl { get; set; } = "";

    public TelegramSettings Telegram { get; set; } = new();
    public FloketSettings Floket { get; set; } = new();
    public DownloaderSettings Downloader { get; set; } = new();
    public ApiSettings Api { get; set; } = new();
    public LoggingSettings Logging { get; set; } = new();
}

public sealed class TelegramSettings
{
    public string BotToken { get; set; } = "";

    public bool EnablePolling { get; set; } = true;
    public string[] AdminUserIds { get; set; } = [];
    public string DefaultLanguage { get; set; } = "en";
}

public sealed class FloketSettings
{
    public int VerificationTimeoutSeconds { get; set; } = 120;

    public int MaxAttempts { get; set; } = 3;

    public bool RestrictUntilVerified { get; set; } = true;

    public bool EnabledByDefault { get; set; } = true;
}

public sealed class DownloaderSettings
{
    public string OutputDirectory { get; set; } = "downloads";
    public string? YtDlpPath { get; set; }
    public string? FFmpegPath { get; set; }
}

public sealed class ApiSettings
{
    public string Host { get; set; } = "http://localhost:5080";
    public bool RequireApiKey { get; set; } = true;
    public string ApiKey { get; set; } = "";
}

public sealed class LoggingSettings
{
    public string MinimumLevel { get; set; } = "Information";
    public string LogDirectory { get; set; } = "logs";
}

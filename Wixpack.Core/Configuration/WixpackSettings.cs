namespace Wixpack.Core.Configuration;

/// <summary>
/// Root application settings bound from settings.json.
/// LogoUrl and DonationUrl are intentionally left empty for the owner to fill.
/// </summary>
public sealed class WixpackSettings
{
    public const string SectionName = "Wixpack";

    /// <summary>URL of the Wixpack logo. Leave empty until set by owner.</summary>
    public string LogoUrl { get; set; } = "";

    /// <summary>Donation / support URL. Leave empty until set by owner.</summary>
    public string DonationUrl { get; set; } = "";

    public TelegramSettings Telegram { get; set; } = new();
    public FloketSettings Floket { get; set; } = new();
    public DownloaderSettings Downloader { get; set; } = new();
    public ApiSettings Api { get; set; } = new();
    public LoggingSettings Logging { get; set; } = new();
}

public sealed class TelegramSettings
{
    /// <summary>Bot token from @BotFather. Prefer environment variable or secret store in production.</summary>
    public string BotToken { get; set; } = "";

    public bool EnablePolling { get; set; } = true;
    public string[] AdminUserIds { get; set; } = [];
    public string DefaultLanguage { get; set; } = "en";
}

public sealed class FloketSettings
{
    /// <summary>Default verification timeout in seconds.</summary>
    public int VerificationTimeoutSeconds { get; set; } = 120;

    /// <summary>Maximum failed attempts before kick/ban action.</summary>
    public int MaxAttempts { get; set; } = 3;

    /// <summary>Whether to restrict new members until verified.</summary>
    public bool RestrictUntilVerified { get; set; } = true;

    /// <summary>Whether verification is enabled by default for new groups.</summary>
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

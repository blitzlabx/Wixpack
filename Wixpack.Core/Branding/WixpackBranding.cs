namespace Wixpack.Core.Branding;

/// <summary>
/// Official branding constants for Wixpack by Blitz.
/// Do not hardcode personal links or fake logo/donation URLs elsewhere.
/// </summary>
public static class WixpackBranding
{
    public const string ProductName = "Wixpack";
    public const string FullProductName = "Wixpack by Blitz";
    public const string Creator = "Blitz";
    public const string SocialHandle = "blitzlabx";
    public const string Attribution = "Wixpack by Blitz · @blitzlabx";

    public static string VersionBanner(string version) =>
        $"{FullProductName} v{version} · Creator: {Creator} · @{SocialHandle}";
}

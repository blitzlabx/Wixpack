using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using QRCoder;
using Wixpack.Core.Models;

namespace Wixpack.DevTools.Services;

public sealed class DevToolsService
{
    public Result<string> FormatJson(string input)
    {
        try
        {
            using var doc = JsonDocument.Parse(input);
            return Result<string>.Ok(JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception ex)
        {
            return Result<string>.Fail($"Invalid JSON: {ex.Message}");
        }
    }

    public Result<string> MinifyJson(string input)
    {
        try
        {
            using var doc = JsonDocument.Parse(input);
            return Result<string>.Ok(JsonSerializer.Serialize(doc));
        }
        catch (Exception ex)
        {
            return Result<string>.Fail($"Invalid JSON: {ex.Message}");
        }
    }

    public string Base64Encode(string input) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(input));

    public Result<string> Base64Decode(string input)
    {
        try
        {
            return Result<string>.Ok(Encoding.UTF8.GetString(Convert.FromBase64String(input.Trim())));
        }
        catch (Exception ex)
        {
            return Result<string>.Fail(ex.Message);
        }
    }

    public string UrlEncode(string input) => Uri.EscapeDataString(input);
    public string UrlDecode(string input) => Uri.UnescapeDataString(input);

    public string NewGuid() => Guid.NewGuid().ToString();

    public string Hash(string input, string algorithm)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        byte[] hash = algorithm.ToUpperInvariant() switch
        {
            "MD5" => MD5.HashData(bytes),
            "SHA1" => SHA1.HashData(bytes),
            "SHA256" => SHA256.HashData(bytes),
            "SHA512" => SHA512.HashData(bytes),
            _ => SHA256.HashData(bytes)
        };
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public Result<object> TestRegex(string pattern, string input, bool ignoreCase = true)
    {
        try
        {
            var opts = ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
            var rx = new Regex(pattern, opts, TimeSpan.FromSeconds(2));
            var matches = rx.Matches(input).Select(m => new { m.Value, m.Index, m.Length }).ToList();
            return Result<object>.Ok(new { isMatch = rx.IsMatch(input), matchCount = matches.Count, matches });
        }
        catch (Exception ex)
        {
            return Result<object>.Fail(ex.Message);
        }
    }

    public Result<object> DecodeJwt(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(token))
                return Result<object>.Fail("Not a readable JWT");
            var jwt = handler.ReadJwtToken(token);
            return Result<object>.Ok(new
            {
                header = jwt.Header,
                payload = jwt.Payload,
                validFrom = jwt.ValidFrom,
                validTo = jwt.ValidTo
            });
        }
        catch (Exception ex)
        {
            return Result<object>.Fail(ex.Message);
        }
    }

    public object TimestampNow()
    {
        var now = DateTimeOffset.UtcNow;
        return new
        {
            unixSeconds = now.ToUnixTimeSeconds(),
            unixMilliseconds = now.ToUnixTimeMilliseconds(),
            iso8601 = now.ToString("O"),
            utc = now.UtcDateTime.ToString("u")
        };
    }

    public Result<object> TimestampConvert(long value, bool milliseconds = false)
    {
        try
        {
            var dto = milliseconds
                ? DateTimeOffset.FromUnixTimeMilliseconds(value)
                : DateTimeOffset.FromUnixTimeSeconds(value);
            return Result<object>.Ok(new { iso8601 = dto.ToString("O"), utc = dto.UtcDateTime.ToString("u") });
        }
        catch (Exception ex)
        {
            return Result<object>.Fail(ex.Message);
        }
    }

    public byte[] GenerateQrPng(string content, int pixelsPerModule = 10)
    {
        using var gen = new QRCodeGenerator();
        using var data = gen.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
        var png = new PngByteQRCode(data);
        return png.GetGraphic(pixelsPerModule);
    }
}

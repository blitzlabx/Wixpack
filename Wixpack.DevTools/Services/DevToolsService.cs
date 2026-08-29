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
    private static readonly char[] PasswordAlphabet =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*-_=+".ToCharArray();

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
    public string HtmlEncode(string input) => System.Net.WebUtility.HtmlEncode(input);
    public string HtmlDecode(string input) => System.Net.WebUtility.HtmlDecode(input);

    public string NewGuid() => Guid.NewGuid().ToString();

    public string[] BulkUuid(int count)
    {
        count = Math.Clamp(count, 1, 50);
        return Enumerable.Range(0, count).Select(_ => Guid.NewGuid().ToString()).ToArray();
    }

    public string Hash(string input, string algorithm)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        byte[] hash = algorithm.ToUpperInvariant() switch
        {
            "MD5" => MD5.HashData(bytes),
            "SHA1" => SHA1.HashData(bytes),
            "SHA256" => SHA256.HashData(bytes),
            "SHA384" => SHA384.HashData(bytes),
            "SHA512" => SHA512.HashData(bytes),
            _ => SHA256.HashData(bytes)
        };
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public object HashAll(string input) => new
    {
        md5 = Hash(input, "MD5"),
        sha1 = Hash(input, "SHA1"),
        sha256 = Hash(input, "SHA256"),
        sha384 = Hash(input, "SHA384"),
        sha512 = Hash(input, "SHA512")
    };

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

    public string GeneratePassword(int length = 16)
    {
        length = Math.Clamp(length, 8, 64);
        var bytes = RandomNumberGenerator.GetBytes(length);
        var chars = new char[length];
        for (var i = 0; i < length; i++)
            chars[i] = PasswordAlphabet[bytes[i] % PasswordAlphabet.Length];
        return new string(chars);
    }

    public string Slugify(string input)
    {
        var s = input.Trim().ToLowerInvariant();
        s = Regex.Replace(s, @"[^a-z0-9\s-]", "");
        s = Regex.Replace(s, @"\s+", "-");
        s = Regex.Replace(s, @"-+", "-").Trim('-');
        return s;
    }

    public string ToCase(string input, string mode) => mode.ToLowerInvariant() switch
    {
        "upper" => input.ToUpperInvariant(),
        "lower" => input.ToLowerInvariant(),
        "title" => System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(input.ToLowerInvariant()),
        "snake" => Regex.Replace(Slugify(input).Replace('-', '_'), @"_+", "_"),
        "camel" => ToCamel(input),
        _ => input
    };

    private static string ToCamel(string input)
    {
        var parts = Regex.Split(input.Trim(), @"[\s_\-]+")
            .Where(p => p.Length > 0).ToArray();
        if (parts.Length == 0) return input;
        return parts[0].ToLowerInvariant() +
               string.Concat(parts.Skip(1).Select(p =>
                   char.ToUpperInvariant(p[0]) + p[1..].ToLowerInvariant()));
    }

    public Result<object> ColorConvert(string input)
    {
        input = input.Trim();
        try
        {
            int r, g, b;
            if (input.StartsWith('#'))
            {
                var hex = input.TrimStart('#');
                if (hex.Length == 3)
                    hex = string.Concat(hex.Select(c => $"{c}{c}"));
                if (hex.Length != 6)
                    return Result<object>.Fail("HEX must be #RGB or #RRGGBB");
                r = Convert.ToInt32(hex[..2], 16);
                g = Convert.ToInt32(hex[2..4], 16);
                b = Convert.ToInt32(hex[4..6], 16);
            }
            else if (input.StartsWith("rgb", StringComparison.OrdinalIgnoreCase))
            {
                var m = Regex.Match(input, @"(\d+)\D+(\d+)\D+(\d+)");
                if (!m.Success) return Result<object>.Fail("Invalid rgb()");
                r = int.Parse(m.Groups[1].Value);
                g = int.Parse(m.Groups[2].Value);
                b = int.Parse(m.Groups[3].Value);
            }
            else
                return Result<object>.Fail("Use #HEX or rgb(r,g,b)");

            r = Math.Clamp(r, 0, 255);
            g = Math.Clamp(g, 0, 255);
            b = Math.Clamp(b, 0, 255);
            var hexOut = $"#{r:X2}{g:X2}{b:X2}";
            return Result<object>.Ok(new { hex = hexOut, rgb = $"rgb({r},{g},{b})", r, g, b });
        }
        catch (Exception ex)
        {
            return Result<object>.Fail(ex.Message);
        }
    }

    public string Lorem(int words = 30)
    {
        words = Math.Clamp(words, 1, 200);
        const string pool =
            "lorem ipsum dolor sit amet consectetur adipiscing elit sed do eiusmod tempor incididunt ut labore et dolore magna aliqua";
        var arr = pool.Split(' ');
        return string.Join(' ', Enumerable.Range(0, words).Select(i => arr[i % arr.Length]));
    }

    public object StringStats(string input) => new
    {
        length = input.Length,
        words = Regex.Matches(input.Trim(), @"\S+").Count,
        lines = input.Split('\n').Length,
        digits = input.Count(char.IsDigit),
        letters = input.Count(char.IsLetter)
    };
}

using CMS.Application.Common.Abstractions;
using System.Security.Cryptography;
using System.Text;

namespace CMS.Infrastructure.Services;

/// <summary>
/// TOTP (Time-based One-Time Password) servisi - RFC 6238 uyumlu implementasyon.
/// Üretim ortamında OtpNet gibi doğrulanmış bir kütüphane kullanılması tavsiye edilir.
/// </summary>
public class TotpService : ITotpService
{
    private const int BackupCodeLength = 8;
    private const int StepSeconds = 30;
    private const int Digits = 6;
    private const int WindowSize = 1; // ±1 zaman penceresi toleransı

    public string GenerateSecret()
    {
        var bytes = RandomNumberGenerator.GetBytes(20);
        return ToBase32(bytes);
    }

    public string[] GenerateBackupCodes(int count)
    {
        var codes = new string[count];
        for (int i = 0; i < count; i++)
        {
            var bytes = RandomNumberGenerator.GetBytes(5);
            codes[i] = BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }
        return codes;
    }

    public string GetQrCodeUri(string email, string secret, string issuer)
    {
        var encodedIssuer = Uri.EscapeDataString(issuer);
        var encodedEmail = Uri.EscapeDataString(email);
        return $"otpauth://totp/{encodedIssuer}:{encodedEmail}?secret={secret}&issuer={encodedIssuer}&digits={Digits}&period={StepSeconds}";
    }

    public bool ValidateCode(string secret, string code)
    {
        if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(code))
            return false;

        var secretBytes = FromBase32(secret);
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / StepSeconds;

        for (long offset = -WindowSize; offset <= WindowSize; offset++)
        {
            var expected = GenerateTotp(secretBytes, timestamp + offset);
            if (expected == code.Trim())
                return true;
        }
        return false;
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private static string GenerateTotp(byte[] secret, long counter)
    {
        var counterBytes = BitConverter.GetBytes(counter);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(counterBytes);

        using var hmac = new HMACSHA1(secret);
        var hash = hmac.ComputeHash(counterBytes);
        var offset = hash[^1] & 0x0F;
        var code = ((hash[offset] & 0x7F) << 24)
                   | ((hash[offset + 1] & 0xFF) << 16)
                   | ((hash[offset + 2] & 0xFF) << 8)
                   | (hash[offset + 3] & 0xFF);

        return (code % (int)Math.Pow(10, Digits)).ToString().PadLeft(Digits, '0');
    }

    private static string ToBase32(byte[] data)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var result = new StringBuilder();
        int buffer = 0, bitsLeft = 0;
        foreach (var b in data)
        {
            buffer = (buffer << 8) | b;
            bitsLeft += 8;
            while (bitsLeft >= 5)
            {
                bitsLeft -= 5;
                result.Append(alphabet[(buffer >> bitsLeft) & 0x1F]);
            }
        }
        if (bitsLeft > 0)
            result.Append(alphabet[(buffer << (5 - bitsLeft)) & 0x1F]);
        return result.ToString();
    }

    private static byte[] FromBase32(string input)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var cleaned = input.ToUpperInvariant().Replace(" ", "").Replace("-", "");
        var result = new List<byte>();
        int buffer = 0, bitsLeft = 0;
        foreach (var c in cleaned)
        {
            var idx = alphabet.IndexOf(c);
            if (idx < 0) continue;
            buffer = (buffer << 5) | idx;
            bitsLeft += 5;
            if (bitsLeft >= 8)
            {
                bitsLeft -= 8;
                result.Add((byte)((buffer >> bitsLeft) & 0xFF));
            }
        }
        return result.ToArray();
    }
}

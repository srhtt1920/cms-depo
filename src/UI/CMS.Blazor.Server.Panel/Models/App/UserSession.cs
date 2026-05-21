using CMS.Blazor.Server.Panel.Models.Auth;

namespace CMS.Blazor.Server.Panel.Models.App;

/// <summary>
/// Kullanıcı oturumu. [PersistentState] ile circuit genelinde yaşar.
/// ProtectedSessionStorage'a da yedeklenir — refresh sonrası restore edilir.
/// </summary>
public sealed class UserSession
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public Guid? CurrentTenantId { get; set; }
    public Guid? UserId { get; set; }
    public string CurrentTenantName { get; set; } = string.Empty;
    public List<TenantInfo> Tenants { get; set; } = [];
    public HashSet<string> Permissions { get; set; } = [];
    public HashSet<string> Roles { get; set; } = [];
    public int PermissionVersion { get; set; }

    public bool IsAuthenticated => !string.IsNullOrEmpty(Token) && ExpiresAt > DateTime.UtcNow;
    public bool HasTenant => CurrentTenantId.HasValue;
    public bool Can(string key) => Permissions.Contains(key);
    public string DisplayLabel => DisplayName ?? Email;

    /// <summary>
    /// TwoFactor doğrulama beklenirken e-postayı geçici saklar.
    /// Login → /two-factor geçişinde kullanılır.
    /// </summary>
    public string? PendingTwoFactorEmail { get; set; }

    /// <summary>
    /// Yeni token çiftini oturuma yazar.
    /// TwoFactor doğrulama başarılı olduğunda çağrılır.
    /// </summary>
    public Task SetTokensAsync(string accessToken, string refreshToken)
    {
        Token = accessToken;
        RefreshToken = refreshToken;
        return Task.CompletedTask; // gerekirse ProtectedSessionStorage'a da yaz
    }

    public string Avatar
    {
        get
        {
            if (string.IsNullOrWhiteSpace(DisplayLabel))
                return "?";

            var value = DisplayLabel.Trim();

            // İçinde boşluk varsa isim soyisim kabul et
            if (value.Contains(' '))
            {
                var parts = value
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length >= 2)
                {
                    return $"{char.ToUpper(parts[0][0])}{char.ToUpper(parts[1][0])}";
                }

                return char.ToUpper(parts[0][0]).ToString();
            }

            // Email veya tek kelime: ilk 2 harf
            return new string(
                value
                    .Take(2)
                    .Select(char.ToUpper)
                    .ToArray()
            );
        }
    }

}

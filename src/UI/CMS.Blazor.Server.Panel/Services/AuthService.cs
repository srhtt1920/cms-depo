using CMS.Blazor.Server.Panel.Infrastructure.ApiClients.Auth;
using CMS.Blazor.Server.Panel.Models.App;
using CMS.Blazor.Server.Panel.Models.Auth;
using CMS.Blazor.Server.Panel.Services.State;
using Microsoft.AspNetCore.Components;

namespace CMS.Blazor.Server.Panel.Services;

public sealed class AuthService(
    AuthApiClient api,
    ApplicationState appState,
    NavigationManager nav)
{
    public async Task<(bool Ok, string? Error, bool NeedsTenantSelect, bool NeedsTwoFactor)> LoginAsync(
        string email, string password, CancellationToken ct = default)
    {
        var result = await api.LoginAsync(new LoginRequest(email, password), ct);
        if (!result.IsSuccess || result.Data is null)
            return (false, result.Error?.Message ?? "Giriş başarısız.", false, false);

        var data = result.Data;

        // Orphan user: hiç tenant'a bağlı değilse erişim yok
        if (!data.RequiresTwoFactor && data.Tenants.Count == 0)
            return (false, "Hesabınıza bağlı aktif bir tenant bulunamadı. Lütfen sistem yöneticinize başvurun.", false, false);

        // API sunucusu 2FA gerektirdiğini bildirirse token kaydetmeden yönlendir
        if (data.RequiresTwoFactor)
        {
            appState.Session.PendingTwoFactorEmail = email;
            return (true, null, false, true);
        }

        var session = new UserSession
        {
            Token = data.Token,
            RefreshToken = data.RefreshToken ?? string.Empty,
            ExpiresAt = data.ExpiresAt,
            Email = data.Email,
            DisplayName = data.DisplayName,
            Tenants = [.. data.Tenants]
        };
        await appState.SetSessionAsync(session);

        if (data.Tenants.Count == 1)
        {
            var (ok, err) = await SelectTenantAsync(data.Tenants[0].TenantId, ct);
            return ok ? (true, null, false, false) : (false, err, false, false);
        }
        return (true, null, true, false);
    }

    public async Task<(bool Ok, string? Error)> SelectTenantAsync(
        Guid tenantId, CancellationToken ct = default)
    {
        var result = await api.SelectTenantAsync(tenantId, ct);
        if (!result.IsSuccess || result.Data is null)
            return (false, result.Error?.Message ?? "Tenant seçimi başarısız.");

        var data = result.Data;
        var permResult = await api.GetPermissionsAsync(data.Token, ct);
        var permissions = new HashSet<string>();
        var permVersion = 0;
        if (permResult.IsSuccess && permResult.Data is not null)
        {
            permissions = PermissionSyncService.FlattenGranted(permResult.Data.Tree);
            permVersion = permResult.Data.Version;
        }
        await appState.UpdateSessionAsync(
            data.TenantId, data.Name,
            data.Token, data.ExpiresAt,
            permissions, permVersion);
        return (true, null);
    }

    /// <summary>
    /// Token yenile. Süresi dolmak üzere veya 401 alındığında çağrılır.
    /// Başarısız olursa logout yapar.
    /// </summary>
    public async Task<bool> TryRefreshAsync(CancellationToken ct = default)
    {
        var refreshToken = appState.Session.RefreshToken;
        if (string.IsNullOrEmpty(refreshToken)) { await LogoutAsync(ct); return false; }

        var result = await api.RefreshTokenAsync(refreshToken, ct);
        if (!result.IsSuccess || result.Data is null) { await LogoutAsync(ct); return false; }

        var data = result.Data;
        var session = appState.Session;
        session.Token = data.Token;
        session.RefreshToken = data.RefreshToken ?? string.Empty;
        session.ExpiresAt = data.ExpiresAt;
        await appState.SetSessionAsync(session);
        return true;
    }

    /// <summary>
    /// İki faktörlü doğrulama kodu doğrular ve tam oturumu kurar.
    /// TwoFactor.razor tarafından çağrılır.
    /// </summary>
    public async Task<(bool Ok, string? Error, bool NeedsTenantSelect)> TwoFactorCompleteAsync(
        string code, string email, CancellationToken ct = default)
    {
        var result = await api.TwoFactorVerifyAsync(new TwoFactorVerifyRequest(code, email), ct);
        if (!result.IsSuccess || result.Data is null)
            return (false, result.Error?.Message ?? "Geçersiz veya süresi dolmuş kod.", false);

        var data = result.Data;
        var session = new UserSession
        {
            Token = data.AccessToken,
            RefreshToken = data.RefreshToken,
            ExpiresAt = data.ExpiresAt,
            Email = data.Email,
            DisplayName = data.DisplayName,
            Tenants = [.. data.Tenants]
        };
        session.PendingTwoFactorEmail = null;
        await appState.SetSessionAsync(session);

        if (data.Tenants.Count == 0)
            return (false, "Hesabınıza bağlı aktif bir tenant bulunamadı.", false);

        if (data.Tenants.Count == 1)
        {
            var (ok, err) = await SelectTenantAsync(data.Tenants[0].TenantId, ct);
            return ok ? (true, null, false) : (false, err, false);
        }
        return (true, null, data.Tenants.Count > 1);
    }

    public async Task LogoutAsync(CancellationToken ct = default)
    {
        var refreshToken = appState.Session.RefreshToken;
        if (!string.IsNullOrEmpty(refreshToken))
            await api.LogoutAsync(refreshToken, ct);
        await appState.LogoutAsync();
        nav.NavigateTo("/login", forceLoad: false);
    }
}

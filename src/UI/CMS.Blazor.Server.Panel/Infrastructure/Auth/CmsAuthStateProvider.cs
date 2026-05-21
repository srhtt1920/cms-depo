using CMS.Blazor.Server.Panel.Infrastructure.Preferences;
using CMS.Blazor.Server.Panel.Models.App;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Claims;

namespace CMS.Blazor.Server.Panel.Infrastructure.Auth;

public sealed class CmsAuthStateProvider(
    ProtectedSessionStorage sessionStorage,
    CookiePreferencesService cookiePrefs) : AuthenticationStateProvider
{
    private const string SessionKey = "cms_session";

    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    private UserSession _session = new();
    private AppPreferences _prefs = new();

    public UserSession Session => _session;
    public AppPreferences Preferences => _prefs;

    // ── AuthenticationStateProvider ───────────────────────────────

    public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
        Task.FromResult(new AuthenticationState(BuildPrincipal(_session)));

    // ── Restore — OnAfterRenderAsync(firstRender) çağrılır ────────

    public async Task RestoreSessionAsync()
    {
        // 1. Tercihler — Cookie'den (her zaman güvenli, ProtectedStorage gerekmez)
        _prefs = await cookiePrefs.LoadAsync();

        // 2. Session — ProtectedSessionStorage (şifreli, tab kapanınca silinir)
        try
        {
            var result = await sessionStorage.GetAsync<UserSession>(SessionKey);
            if (result.Success && result.Value?.IsAuthenticated == true)
            {
                _session = result.Value;
                NotifyAuthenticationStateChanged(
                    Task.FromResult(new AuthenticationState(BuildPrincipal(_session))));
            }
        }
        catch { /* Bozuk şifreli veri — temizle */ }
    }

    // ── Session ──────────────────────────────────────────────────

    public async Task SetSessionAsync(UserSession session)
    {
        _session = session;
        await sessionStorage.SetAsync(SessionKey, session);
        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(BuildPrincipal(session))));
    }

    public async Task UpdateSessionAsync(
        Guid tenantId, string tenantName,
        string newToken, DateTime expiresAt,
        HashSet<string> permissions, int permVersion)
    {
        _session.CurrentTenantId = tenantId;
        _session.CurrentTenantName = tenantName;
        _session.Token = newToken;
        _session.ExpiresAt = expiresAt;
        _session.Permissions = permissions;
        _session.PermissionVersion = permVersion;
        await sessionStorage.SetAsync(SessionKey, _session);
        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(BuildPrincipal(_session))));
    }

    public async Task UpdatePermissionsAsync(HashSet<string> permissions, int permVersion)
    {
        _session.Permissions = permissions;
        _session.PermissionVersion = permVersion;
        await sessionStorage.SetAsync(SessionKey, _session);
    }

    public async Task LogoutAsync()
    {
        _session = new();
        await sessionStorage.DeleteAsync(SessionKey);
        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(_anonymous)));
    }

    // ── Preferences (Cookie üzerinden) ───────────────────────────

    public async Task SetThemeAsync(string theme)
    {
        _prefs.Theme = theme;
        await cookiePrefs.SetThemeAsync(theme);
    }

    public async Task SetMenuThemeAsync(string menuTheme)
    {
        _prefs.MenuTheme = menuTheme;
        await cookiePrefs.SetMenuThemeAsync(menuTheme);
    }

    public async Task SetLanguageAsync(string lang)
    {
        _prefs.Language = lang;
        await cookiePrefs.SetLanguageAsync(lang);
    }

    public async Task SetSizeModeAsync(string mode)
    {
        _prefs.SizeMode = mode;
        await cookiePrefs.SetSizeModeAsync(mode);
    }

    // ── ClaimsPrincipal ──────────────────────────────────────────

    private static ClaimsPrincipal BuildPrincipal(UserSession session)
    {
        if (!session.IsAuthenticated)
            return new ClaimsPrincipal(new ClaimsIdentity());

        var email = session.Email ?? string.Empty;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, email),
            new(ClaimTypes.Email,          email),
            new(ClaimTypes.Name,           email),
        };

        if (session.CurrentTenantId.HasValue)
            claims.Add(new("tenantId", session.CurrentTenantId.Value.ToString()));

        foreach (var perm in session.Permissions ?? [])
            claims.Add(new("permission", perm));

        return new ClaimsPrincipal(
            new ClaimsIdentity(claims, authenticationType: "CmsJwt"));
    }
}

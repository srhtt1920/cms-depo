using CMS.Blazor.Server.Panel.Infrastructure.Auth;
using CMS.Blazor.Server.Panel.Models.App;

namespace CMS.Blazor.Server.Panel.Services.State;

public sealed class ApplicationState(CmsAuthStateProvider authProvider)
{
    public event Action? OnThemeChanged;

    public CmsAuthStateProvider AuthProvider => authProvider;

    public UserSession Session => authProvider.Session;
    public AppPreferences Preferences => authProvider.Preferences;

    public Task RestoreAsync() => authProvider.RestoreSessionAsync();
    public Task SetSessionAsync(UserSession s) => authProvider.SetSessionAsync(s);
    public Task LogoutAsync() => authProvider.LogoutAsync();

    public Task UpdateSessionAsync(
        Guid tenantId, string tenantName, string newToken,
        DateTime expiresAt, HashSet<string> permissions, int permVersion)
        => authProvider.UpdateSessionAsync(
               tenantId, tenantName, newToken, expiresAt, permissions, permVersion);

    public Task UpdatePermissionsAsync(HashSet<string> permissions, int permVersion)
        => authProvider.UpdatePermissionsAsync(permissions, permVersion);

    // ── Preferences ──────────────────────────────────────────────
    public async Task SetThemeAsync(string v)
    {
        await authProvider.SetThemeAsync(v);
        OnThemeChanged?.Invoke();
    }

    public Task SetMenuThemeAsync(string v) => authProvider.SetMenuThemeAsync(v);
    public Task SetLanguageAsync(string v) => authProvider.SetLanguageAsync(v);
    public Task SetSizeModeAsync(string v) => authProvider.SetSizeModeAsync(v);
}

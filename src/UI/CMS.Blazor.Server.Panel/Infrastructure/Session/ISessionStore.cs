using CMS.Blazor.Server.Panel.Models.App;

namespace CMS.Blazor.Server.Panel.Infrastructure.Session;

/// <summary>
/// Session storage abstraction.
/// appsettings.json → Session:Provider = "Cookie" | "Memory"
/// </summary>
public interface ISessionStore
{
    Task<UserSession?> LoadAsync(CancellationToken ct = default);
    Task SaveAsync(UserSession session, CancellationToken ct = default);
    Task ClearAsync(CancellationToken ct = default);

    Task<AppPreferences> LoadPreferencesAsync(CancellationToken ct = default);
    Task SavePreferencesAsync(AppPreferences prefs, CancellationToken ct = default);
}

using CMS.Blazor.Server.Panel.Models.App;

namespace CMS.Blazor.Server.Panel.Infrastructure.Session;

/// <summary>
/// In-memory session — Blazor circuit boyunca yaşar.
/// Refresh'te kaybolur, geliştirme/test için uygundur.
/// </summary>
public sealed class MemorySessionStore : ISessionStore
{
    private UserSession? _session;
    private AppPreferences _prefs = new();

    public Task<UserSession?> LoadAsync(CancellationToken ct = default) =>
        Task.FromResult(_session?.IsAuthenticated == true ? _session : null);

    public Task SaveAsync(UserSession session, CancellationToken ct = default)
    {
        _session = session;
        return Task.CompletedTask;
    }

    public Task ClearAsync(CancellationToken ct = default)
    {
        _session = null;
        return Task.CompletedTask;
    }

    public Task<AppPreferences> LoadPreferencesAsync(CancellationToken ct = default) =>
        Task.FromResult(_prefs);

    public Task SavePreferencesAsync(AppPreferences prefs, CancellationToken ct = default)
    {
        _prefs = prefs;
        return Task.CompletedTask;
    }
}

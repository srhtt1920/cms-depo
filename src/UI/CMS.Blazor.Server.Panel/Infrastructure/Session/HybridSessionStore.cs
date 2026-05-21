using CMS.Blazor.Server.Panel.Models.App;

namespace CMS.Blazor.Server.Panel.Infrastructure.Session;

/// <summary>
/// Cookie + Memory hibrit store.
/// İlk yükleme: Cookie'den oku → Memory'e yaz.
/// Sonraki okumalar: Memory'den hızlıca döner.
/// Yazma: hem Memory hem Cookie'ye yazar.
/// Blazor WebSocket aşamasında Cookie yazılamaz; bu yüzden
/// AuthController'daki cookie set işlemini de yönetir.
/// </summary>
public sealed class HybridSessionStore(
    CookieSessionStore cookie,
    MemorySessionStore memory) : ISessionStore
{
    private bool _restored;

    public async Task<UserSession?> LoadAsync(CancellationToken ct = default)
    {
        // Memory'de varsa direkt dön
        var mem = await memory.LoadAsync(ct);
        if (mem is not null) return mem;

        // Henüz restore edilmemişse cookie'den yükle
        if (!_restored)
        {
            _restored = true;
            var fromCookie = await cookie.LoadAsync(ct);
            if (fromCookie is not null)
                await memory.SaveAsync(fromCookie, ct);
            return fromCookie;
        }

        return null;
    }

    public async Task SaveAsync(UserSession session, CancellationToken ct = default)
    {
        await memory.SaveAsync(session, ct);
        await cookie.SaveAsync(session, ct); // HttpContext varsa yazar, yoksa no-op
    }

    public async Task ClearAsync(CancellationToken ct = default)
    {
        await memory.ClearAsync(ct);
        await cookie.ClearAsync(ct);
        _restored = false;
    }

    public Task<AppPreferences> LoadPreferencesAsync(CancellationToken ct = default) =>
        cookie.LoadPreferencesAsync(ct);

    public Task SavePreferencesAsync(AppPreferences prefs, CancellationToken ct = default) =>
        cookie.SavePreferencesAsync(prefs, ct);
}

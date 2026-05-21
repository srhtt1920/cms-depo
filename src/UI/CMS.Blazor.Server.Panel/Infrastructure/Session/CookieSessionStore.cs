using CMS.Blazor.Server.Panel.Models.App;
using System.Text.Json;

namespace CMS.Blazor.Server.Panel.Infrastructure.Session;

/// <summary>
/// HTTPOnly Cookie tabanlı session.
/// Token → "__cms_session" cookie'sine JSON olarak yazılır.
/// Cookie JS tarafından okunamaz (HttpOnly), XSS güvenliği sağlanır.
///
/// Not: Blazor Server'da IHttpContextAccessor WebSocket bağlantısında
/// null döner. Bu nedenle cookie okuma/yazma için AuthController kullanılır.
/// Bu store Blazor tarafında sadece memory cache olarak davranır;
/// gerçek cookie işlemi AuthController üzerinden yapılır.
/// </summary>
public sealed class CookieSessionStore(
    IHttpContextAccessor httpContextAccessor) : ISessionStore
{
    private const string SessionCookie = "__cms_session";
    private const string PrefsCookie = "__cms_prefs";

    private static readonly JsonSerializerOptions Opts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    // Blazor WebSocket aşamasında HttpContext null olabilir.
    // Bu durumda memory fallback kullanılır.
    private HttpContext? Context => httpContextAccessor.HttpContext;

    public Task<UserSession?> LoadAsync(CancellationToken ct = default)
    {
        var ctx = Context;
        if (ctx is null) return Task.FromResult<UserSession?>(null);

        if (!ctx.Request.Cookies.TryGetValue(SessionCookie, out var raw)
            || string.IsNullOrEmpty(raw))
            return Task.FromResult<UserSession?>(null);

        try
        {
            var session = JsonSerializer.Deserialize<UserSession>(raw, Opts);
            return Task.FromResult(session?.IsAuthenticated == true ? session : null);
        }
        catch { return Task.FromResult<UserSession?>(null); }
    }

    public Task SaveAsync(UserSession session, CancellationToken ct = default)
    {
        var ctx = Context;
        if (ctx is null) return Task.CompletedTask;

        var json = JsonSerializer.Serialize(session, Opts);
        ctx.Response.Cookies.Append(SessionCookie, json, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = session.ExpiresAt
        });
        return Task.CompletedTask;
    }

    public Task ClearAsync(CancellationToken ct = default)
    {
        Context?.Response.Cookies.Delete(SessionCookie);
        return Task.CompletedTask;
    }

    public Task<AppPreferences> LoadPreferencesAsync(CancellationToken ct = default)
    {
        var ctx = Context;
        if (ctx?.Request.Cookies.TryGetValue(PrefsCookie, out var raw) == true
            && !string.IsNullOrEmpty(raw))
        {
            try
            {
                var prefs = JsonSerializer.Deserialize<AppPreferences>(raw, Opts);
                if (prefs is not null) return Task.FromResult(prefs);
            }
            catch { }
        }
        return Task.FromResult(new AppPreferences());
    }

    public Task SavePreferencesAsync(AppPreferences prefs, CancellationToken ct = default)
    {
        var ctx = Context;
        if (ctx is null) return Task.CompletedTask;

        var json = JsonSerializer.Serialize(prefs, Opts);
        ctx.Response.Cookies.Append(PrefsCookie, json, new CookieOptions
        {
            HttpOnly = false, // Preferences JS'den okunabilir (tema/dil flash önleme)
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddYears(1)
        });
        return Task.CompletedTask;
    }
}

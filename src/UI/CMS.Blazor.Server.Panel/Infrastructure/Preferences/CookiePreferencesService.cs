using CMS.Blazor.Server.Panel.Models.App;
using Microsoft.JSInterop;

namespace CMS.Blazor.Server.Panel.Infrastructure.Preferences;

/// <summary>
/// Kullanıcı tercihlerini cookie'den okur/yazar.
/// JS Interop — sadece OnAfterRenderAsync içinde çağrılmalı.
///
/// Cookie isimleri:
///   cms-appTheme  : light | dark
///   cms-language  : tr | en | de
///   cms-menuTheme : dark | light | colored
///   cms-sizeMode  : default | compact | comfortable
/// </summary>
public sealed class CookiePreferencesService(IJSRuntime js)
{
    private const int ExpireDays = 365;

    // ── Toplu okuma ──────────────────────────────────────────────

    public async Task<AppPreferences> LoadAsync()
    {
        try
        {
            // getAll tek JS çağrısında tüm cookie'leri döner — round trip optimize
            var raw = await js.InvokeAsync<RawPrefs>("cmsPrefs.getAll");
            return new AppPreferences
            {
                Theme = raw.Theme ?? "light",
                Language = raw.Language ?? "tr",
                MenuTheme = raw.MenuTheme ?? "dark",
                SizeMode = raw.SizeMode ?? "default"
            };
        }
        catch { return new AppPreferences(); }
    }

    // ── Toplu yazma ──────────────────────────────────────────────

    public async Task SaveAsync(AppPreferences prefs)
    {
        try
        {
            await js.InvokeVoidAsync("cmsPrefs.saveAll", new
            {
                theme = prefs.Theme,
                language = prefs.Language,
                menuTheme = prefs.MenuTheme,
                sizeMode = prefs.SizeMode
            });
            // HTML attribute'ları da güncelle
            await js.InvokeVoidAsync("cmsPrefs.applyAll");
        }
        catch { }
    }

    // ── Tekil yazma (hızlı toggle için) ─────────────────────────

    public async Task SetThemeAsync(string theme)
    {
        await SetCookieAsync("cms-appTheme", theme);
        await js.InvokeVoidAsync("document.documentElement.setAttribute", "data-bs-theme", theme);
    }

    public async Task SetMenuThemeAsync(string menuTheme)
    {
        await SetCookieAsync("cms-menuTheme", menuTheme);
        await js.InvokeVoidAsync("document.documentElement.setAttribute", "data-menu-theme", menuTheme);
    }

    public async Task SetLanguageAsync(string lang)
    {
        await SetCookieAsync("cms-language", lang);
        await js.InvokeVoidAsync("document.documentElement.setAttribute", "lang", lang);
    }

    public async Task SetSizeModeAsync(string mode)
    {
        await SetCookieAsync("cms-sizeMode", mode);
        await js.InvokeVoidAsync("document.documentElement.setAttribute", "data-size-mode", mode);
    }

    // ── Private ──────────────────────────────────────────────────

    private async Task SetCookieAsync(string name, string value)
    {
        try { await js.InvokeVoidAsync("cmsPrefs.setCookie", name, value, ExpireDays); }
        catch { }
    }

    // JS'den gelen ham veri
    private sealed record RawPrefs(
        string? Theme,
        string? Language,
        string? MenuTheme,
        string? SizeMode);
}

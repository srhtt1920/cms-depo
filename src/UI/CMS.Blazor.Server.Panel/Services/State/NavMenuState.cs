using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace CMS.Blazor.Server.Panel.Services.State;

// ── Veri modelleri ────────────────────────────────────────────────────────────

/// Menüde görüntülenen tek bir bağlantı
public sealed record NavMenuEntry(
    string Id,            // benzersiz kimlik — built-in'ler sabit, custom GUID
    string Label,         // Görünen ad
    string Href,          // Yönlendirme URL'i
    string Icon,          // SvgIcon name
    string TabKey,        // Hangi tabda görünsün
    string RequiredPerm,  // "" = herkese açık; izin kontrolü buna göre yapılır
    int Order,         // Sıralama (küçük = üstte)
    bool IsBuiltIn,     // true = silinemiyor; sadece sıra/görünürlük değiştirilebilir
    bool IsVisible = true,
    string? Badge = null,
    string BadgeCss = "");

/// Tab (üst sekme) tanımı
public sealed record NavTabDef(
    string Key,
    string Label,
    string Icon,
    string RequiredPerm,   // tab görünmek için gereken izin
    int Order);

/// Kullanıcı tarafından kayıt edilen özelleştirmeler
public sealed record NavCustomization(
    string Id,
    bool IsVisible,
    int Order,
    string? CustomLabel);

// ── Servis ───────────────────────────────────────────────────────────────────

/// <summary>
/// Menü yapısını yönetir.
/// • Built-in öğeleri yetki filtresiyle sunar
/// • Kullanıcı özelleştirmelerini (sıra, gizle, özel etiket) browser storage'a kaydeder
/// • OnChanged event'i ile layout reaktif güncelleme alır
/// Program.cs'e AddScoped olarak kayıt edin.
/// </summary>
public sealed class NavMenuState
{
    private readonly ApplicationState _app;
    private readonly ProtectedLocalStorage _storage;

    private const string StorageKey = "cms_nav_v2";

    private Dictionary<string, NavCustomization> _custom = [];
    private List<NavMenuEntry> _customEntries = [];   // kullanıcı eklemeleri
    private bool _loaded;

    public event Action? OnChanged;

    // ── Built-in tab tanımları ────────────────────────────────────────────────

    public static readonly List<NavTabDef> BuiltInTabs =
    [
        new("dashboard",  "Dashboard", "home", string.Empty,                 0),
        new("content",    "İçerik",    "file-text","content.read",    1),
        new("seo",        "SEO & Yapı","search",   "content.read",    2),
        new("management", "Yönetim",   "settings", "users.manage",    3),
        new("account",    "Hesap",     "user", string.Empty,                4),
    ];

    // ── Built-in menü öğeleri ─────────────────────────────────────────────────

    public static readonly List<NavMenuEntry> BuiltInEntries =
    [
        // Dashboard
        new("dashboard.home",         "Dashboard",             "/",                            "icon-home",      "dashboard", string.Empty,                    0, true),
        new("dashboard.schedule_cal", "Yayın Takvimi",         "/schedule/calendar",           "icon-date",      "dashboard",   "content.publish",     1, true),
        // Sayfa
        new("page.list",              "Sayfalar",              "/pages",                       "icon-page",      "page",        "page.read",           0, true),
        new("page.wizard",            "Sayfa Oluşturma",       "/pages/wizard",                "icon-p-wizard",  "content",     "page.create",         1, true),
        // İçerik                                              
        new("content.list",           "İçerikler",             "/contents",                    "icon-file-text", "content",     "content.read",        1, true),
        new("content.create",         "Yeni İçerik",           "/contents/create",             "icon-plus",      "content",     "content.create",      2, true),
        new("content.trash",          "Çöp Kutusu",            "/contents/trash",              "icon-trash",     "content",     "content.readDeleted", 3, true),
        new("content.archive",        "Arşiv",                 "/contents/archive",            "icon-collection","content",     "content.read",        4, true),
        new("content.schedule",       "Zamanlama",             "/schedule",                    "icon-alarm",     "content",     "content.publish",     5, true),
        new("content.translations",   "Çeviri Yönetimi",       "/translations",                "icon-translate", "content",     "content.read",        6, true),
        new("content.approvals",      "Onay Merkezi",          "/approval-center",             "icon-shield",    "content",     "content.publish",     7, true),
        new("content.locks",          "Kilit Yönetimi",        "/lock-dashboard",              "icon-key",       "content",     "content.publish",     8, true),
        // SEO                                         
        new("seo.seo",                "SEO & Sitemap",         "/seo",                         "icon-globe",     "seo",         "content.read",        0, true),
        // Yönetim                                             
        new("mgmt.users",             "Kullanıcılar",          "/users",                       "icon-persons",   "management",  "users.manage",        0, true),
        new("mgmt.invites",           "Davetler",              "/users/invites",               "icon-mail",      "management",  "users.manage",        1, true),
        new("mgmt.roles",             "Roller",                "/roles",                       "icon-shield",    "management",  "roles.manage",        1, true),
        new("mgmt.perms",             "İzin Matrisi",          "/permissions/matrix",          "icon-permission","management",  "roles.manage",        2, true),
        new("mgmt.tenants",           "Tenantlar",             "/tenants",                     "icon-tenant",    "management",  "tenants.manage",      3, true),
        new("mgmt.auditlog",          "Audit Log",             "/audit-logs",                  "icon-collection","management",  "tenants.manage",      4, true),
        new("mgmt.navmenu",           "Menü Editörü",          "/settings/nav",                "icon-menu",      "management",  "tenants.manage",      5, true),
        new("mgmt.nav",               "Menü Düzenleyici",      "/settings/nav-editor",         "icon-menu2",     "management",  "users.manage",        6, true),
        new("mgmt.perm_adv",          "Gelişmiş İzin Matrisi", "/permissions/matrix/advanced", "icon-key",       "management",  "roles.manage",        7, true),
        // Hesap
        new("account.profile",        "Profil",                "/profile",                     "icon-user",      "account", string.Empty,                    0, true),
        new("account.security",       "Güvenlik",              "/security",                    "icon-shield",    "account", string.Empty,                    1, true),
    ];

    public NavMenuState(ApplicationState app, ProtectedLocalStorage storage)
    {
        _app = app;
        _storage = storage;
    }

    // ── Yükleme ───────────────────────────────────────────────────────────────

    public async Task EnsureLoadedAsync()
    {
        if (_loaded) return;
        try
        {
            var r = await _storage.GetAsync<NavStoragePayload>(StorageKey);
            if (r.Success && r.Value is not null)
            {
                _custom = r.Value.Customizations.ToDictionary(c => c.Id);
                _customEntries = r.Value.CustomEntries ?? [];
            }
        }
        catch { /* Storage henüz dolmamış veya şifreli veri bozuk */ }
        _loaded = true;
    }

    private async Task SaveAsync()
    {
        var payload = new NavStoragePayload(
            [.. _custom.Values],
            _customEntries);
        await _storage.SetAsync(StorageKey, payload);
        OnChanged?.Invoke();
    }

    // ── Sorgu ─────────────────────────────────────────────────────────────────

    /// Aktif oturumda görünmesi gereken tabları döner
    public List<NavTabDef> GetVisibleTabs()
    {
        var hasPerm = _app.Session.Can;
        return [.. BuiltInTabs
            .Where(t => string.IsNullOrEmpty(t.RequiredPerm) || hasPerm(t.RequiredPerm))
            .OrderBy(t => CustomOrder("tab_" + t.Key, t.Order))
            .Where(t => IsVisible("tab_" + t.Key))];
    }

    /// Belirli bir tab için görünen öğeleri döner
    public List<NavMenuEntry> GetVisibleEntries(string tabKey)
    {
        var hasPerm = _app.Session.Can;

        var built = BuiltInEntries
            .Where(e => e.TabKey == tabKey)
            .Where(e => string.IsNullOrEmpty(e.RequiredPerm) || hasPerm(e.RequiredPerm))
            .Where(e => IsVisible(e.Id))
            .Select(e => ApplyCustomization(e))
            .ToList();

        var custom = _customEntries
            .Where(e => e.TabKey == tabKey)
            .Where(e => string.IsNullOrEmpty(e.RequiredPerm) || hasPerm(e.RequiredPerm))
            .Where(e => IsVisible(e.Id))
            .Select(e => ApplyCustomization(e))
            .ToList();

        return [.. built
        .Concat(custom)
        .OrderBy(e => CustomOrder(e.Id, e.Order))];
    }

    /// Tüm öğeleri (editor için) — gizliler dahil, izin filtresi olmadan
    public List<NavMenuEntry> GetAllEntries(string tabKey)
    {
        var built = BuiltInEntries.Where(e => e.TabKey == tabKey).Select(ApplyCustomization);
        var custom = _customEntries.Where(e => e.TabKey == tabKey).Select(ApplyCustomization);

        return [.. built
            .Concat(custom)
            .OrderBy(e => CustomOrder(e.Id, e.Order))];
    }

    public NavCustomization? GetCustomization(string id) =>
        _custom.TryGetValue(id, out var c) ? c : null;

    // ── Değiştirme ────────────────────────────────────────────────────────────

    public async Task SetVisibilityAsync(string id, bool visible)
    {
        Upsert(id, c => c with { IsVisible = visible });
        await SaveAsync();
    }

    public async Task SetOrderAsync(string id, int order)
    {
        Upsert(id, c => c with { Order = order });
        await SaveAsync();
    }

    public async Task SetLabelAsync(string id, string label)
    {
        Upsert(id, c => c with { CustomLabel = string.IsNullOrWhiteSpace(label) ? null : label });
        await SaveAsync();
    }

    public async Task MoveUpAsync(string id, string tabKey)
    {
        var items = GetAllEntries(tabKey);
        var idx = items.FindIndex(e => e.Id == id);
        if (idx <= 0) return;
        await SwapOrderAsync(items[idx - 1].Id, items[idx].Id);
    }

    public async Task MoveDownAsync(string id, string tabKey)
    {
        var items = GetAllEntries(tabKey);
        var idx = items.FindIndex(e => e.Id == id);
        if (idx < 0 || idx >= items.Count - 1) return;
        await SwapOrderAsync(items[idx].Id, items[idx + 1].Id);
    }

    private async Task SwapOrderAsync(string idA, string idB)
    {
        var oa = CustomOrder(idA, GetEntry(idA)?.Order ?? 0);
        var ob = CustomOrder(idB, GetEntry(idB)?.Order ?? 0);
        Upsert(idA, c => c with { Order = ob });
        Upsert(idB, c => c with { Order = oa });
        await SaveAsync();
    }

    /// Custom link ekle
    public async Task AddCustomEntryAsync(string tabKey, string label, string href, string icon, string perm = "")
    {
        var entry = new NavMenuEntry(
            Guid.NewGuid().ToString("N"), label, href, icon, tabKey, perm,
            _customEntries.Count(e => e.TabKey == tabKey) + 100,
            IsBuiltIn: false);
        _customEntries.Add(entry);
        await SaveAsync();
    }

    /// Custom link sil
    public async Task RemoveCustomEntryAsync(string id)
    {
        _customEntries.RemoveAll(e => e.Id == id);
        _custom.Remove(id);
        await SaveAsync();
    }

    /// Tüm özelleştirmeleri sıfırla
    public async Task ResetAsync()
    {
        _custom.Clear();
        _customEntries.Clear();
        await SaveAsync();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private bool IsVisible(string id) =>
        !_custom.TryGetValue(id, out var c) || c.IsVisible;

    private int CustomOrder(string id, int defaultOrder) =>
        _custom.TryGetValue(id, out var c) ? c.Order : defaultOrder;

    private NavMenuEntry ApplyCustomization(NavMenuEntry e)
    {
        if (!_custom.TryGetValue(e.Id, out var c)) return e;
        var label = string.IsNullOrWhiteSpace(c.CustomLabel) ? e.Label : c.CustomLabel;
        return e with { Label = label, IsVisible = c.IsVisible, Order = c.Order };
    }

    private NavMenuEntry? GetEntry(string id) =>
        BuiltInEntries.FirstOrDefault(e => e.Id == id)
        ?? _customEntries.FirstOrDefault(e => e.Id == id);

    private void Upsert(string id, Func<NavCustomization, NavCustomization> transform)
    {
        var existing = _custom.TryGetValue(id, out var c)
            ? c
            : new NavCustomization(id, IsVisible: true, Order: GetEntry(id)?.Order ?? 99, CustomLabel: null);
        _custom[id] = transform(existing);
    }

    public IEnumerable<NavMenuEntry> GetAllEntriesNoFilter(string tabKey)
        => BuiltInEntries.Where(e => e.TabKey == tabKey);

    public IEnumerable<NavMenuEntry> GetEntriesForPermissions(string tabKey, HashSet<string> grantedPerms)
        => BuiltInEntries
        .Where(e => e.TabKey == tabKey
            && (string.IsNullOrEmpty(e.RequiredPerm)
            || grantedPerms.Contains(e.RequiredPerm)));

    // ── Storage payload ───────────────────────────────────────────────────────

    private sealed record NavStoragePayload(
        List<NavCustomization> Customizations,
        List<NavMenuEntry>? CustomEntries);
}
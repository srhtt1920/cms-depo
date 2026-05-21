using CMS.Blazor.Server.Panel.Infrastructure.ApiClients.Tenant;

namespace CMS.Blazor.Server.Panel.Services.State;

/// <summary>
/// Oturumda seçili tenant'ın desteklediği dilleri tutar.
/// Herhangi bir bileşen <see cref="OnChanged"/> eventi ile değişiklikleri dinleyebilir.
/// Program.cs'de AddScoped olarak kayıt edilmeli.
/// </summary>
public sealed class TenantLanguageState
{
    private readonly ApplicationState _app;
    private readonly TenantApiClient _api;

    /// Tenant'ın desteklediği diller (örn: ["tr","en","de"])
    public IReadOnlyList<string> SupportedLanguages { get; private set; } = [];

    /// Varsayılan dil (örn: "tr")
    public string DefaultLanguage { get; private set; } = "tr";

    /// Son yüklenen tenant id — gereksiz API çağrısı önler
    private Guid? _loadedFor;

    public event Action? OnChanged;

    public TenantLanguageState(ApplicationState app, TenantApiClient api)
    {
        _app = app;
        _api = api;
    }

    /// <summary>
    /// Aktif tenant değiştiyse API'den yeniler. Yoksa cache döner.
    /// </summary>
    public async Task EnsureLoadedAsync(bool forceRefresh = false)
    {
        var tenantId = _app.Session.CurrentTenantId;
        if (tenantId is null)
        {
            SupportedLanguages = [];
            DefaultLanguage = "tr";
            _loadedFor = null;
            return;
        }

        if (!forceRefresh && _loadedFor == tenantId) return;

        // Tüm tenant listesi yerine yalnızca bu tenant'ın settings'ini çek.
        // GetTenantsAsync superadmin yetkisi gerektirir; GetTenantSettingsAsync ise
        // tenant'a dahil olan her kullanıcı için erişilebilir.
        var r = await _api.GetTenantSettingsAsync(tenantId.Value);
        if (r.IsSuccess && r.Data is not null)
        {
            SupportedLanguages = r.Data.SupportedLanguages
                .Select(l => l.LanguageCode)
                .ToList()
                .AsReadOnly();
            DefaultLanguage = r.Data.DefaultLanguageCode;
            _loadedFor = tenantId;
            OnChanged?.Invoke();
        }
    }

    /// <summary>
    /// Verilen dil listesinden tenant'ta eksik olanları döner.
    /// </summary>
    public IReadOnlyList<string> MissingIn(IEnumerable<string> existing)
        => SupportedLanguages
            .Where(l => !existing.Contains(l, StringComparer.OrdinalIgnoreCase))
            .ToList()
            .AsReadOnly();

    /// <summary>
    /// Belirli bir dil tenant tarafından destekleniyor mu?
    /// </summary>
    public bool Supports(string lang)
        => SupportedLanguages.Any(l => l.Equals(lang, StringComparison.OrdinalIgnoreCase));
}

using CMS.Domain.Common;
using CMS.Domain.Tenants.Events;

namespace CMS.Domain.Tenants;

public sealed class Tenant : AggregateRoot<TenantId>, ITenantEntity
{
    private readonly List<SupportedLanguage> _supportedLanguages = [];

    public string        Name                { get; private set; } = default!;
    public string        DefaultLanguageCode { get; private set; } = "en";
    public bool          IsActive            { get; private set; } = true;
    public bool          IsMaintenanceMode   { get; private set; }
    public TenantType    Type                { get; private set; } = TenantType.Business;
    public DateTime      CreatedAt           { get; private set; }
    public DateTime?     UpdatedAt           { get; private set; }

    public bool IsSystem     => Type == TenantType.System;
    public bool IsBusiness => Type == TenantType.Business;

    // ITenantEntity
    Guid ITenantEntity.TenantId => Id.Value;

    public IReadOnlyList<SupportedLanguage> SupportedLanguages =>
        _supportedLanguages.AsReadOnly();

    private Tenant() { } // EF Core

    /// <summary>
    /// Sadece DataSeeder tarafından çağrılır. Root tenant tekil olmalı.
    /// </summary>
    public static Tenant CreateRoot(string name = "System")
        => CreateInternal(name, "tr", TenantType.System);

    public static Tenant CreateBusiness(string name, string langCode)
    => CreateInternal(name, langCode, TenantType.Business);

    private static Tenant CreateInternal(string name, string langCode, TenantType type)
    {
        var tenant = new Tenant
        {
            Id = TenantId.New(),
            Name = name,
            DefaultLanguageCode = langCode.ToLowerInvariant(),
            Type = type,
            CreatedAt = DateTime.UtcNow
        };
        var defaultLang = SupportedLanguage.Create(
            tenant.Id, langCode, isDefault: true, isFallback: true);
        tenant._supportedLanguages.Add(defaultLang);
        tenant.AddDomainEvent(new TenantCreatedEvent(tenant.Id, tenant.Name));
        return tenant;
    }

    // ── Domain Guard — Root Koruması ──────────────────────────────────────

    private void EnsureNotRoot(string operation)
    {
        if (IsSystem)
            throw new InvalidOperationException(
                $"System tenant üzerinde '{operation}' işlemi yasaktır.");
    }

    // ── Mutations ─────────────────────────────────────────────────────────

    public void Rename(string newName)
    {
        EnsureNotRoot("Rename");
        Name = newName;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        EnsureNotRoot("Activate");
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        EnsureNotRoot("Deactivate");
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetMaintenanceMode(bool enabled)
    {
        IsMaintenanceMode = enabled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddSupportedLanguage(string languageCode)
    {
        var normalized = languageCode.ToLowerInvariant();
        if (_supportedLanguages.Any(l => l.LanguageCode == normalized))
            return;
        _supportedLanguages.Add(SupportedLanguage.Create(Id, normalized));
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new TenantLanguageUpdatedEvent(Id));
    }

    public void SetSupportedLanguages(IEnumerable<string> languageCodes, string defaultLanguageCode)
    {
        var codes = languageCodes.Select(c => c.ToLowerInvariant()).ToHashSet();
        var def = defaultLanguageCode.ToLowerInvariant();
        codes.Add(def);

        foreach (var code in codes.Where(
            c => !_supportedLanguages.Any(l => l.LanguageCode == c)))
            _supportedLanguages.Add(SupportedLanguage.Create(Id, code));

        _supportedLanguages.RemoveAll(l => !codes.Contains(l.LanguageCode));

        foreach (var l in _supportedLanguages) { l.ClearDefault(); l.ClearFallback(); }
        var defaultLang = _supportedLanguages.First(l => l.LanguageCode == def);
        defaultLang.SetAsDefault();
        defaultLang.SetAsFallback();

        DefaultLanguageCode = def;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new TenantLanguageUpdatedEvent(Id));
    }

    public void SetDefaultLanguage(string languageCode)
    {
        var lang = _supportedLanguages
            .FirstOrDefault(l => l.LanguageCode == languageCode.ToLowerInvariant())
            ?? throw new InvalidOperationException(
                $"Language '{languageCode}' is not supported by this tenant.");

        foreach (var l in _supportedLanguages) l.ClearDefault();
        lang.SetAsDefault();
        DefaultLanguageCode = languageCode.ToLowerInvariant();
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new TenantLanguageUpdatedEvent(Id));
    }
}

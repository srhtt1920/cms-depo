using CMS.Domain.Common;

namespace CMS.Domain.Tenants;

public sealed class SupportedLanguage : Entity<Guid>
{
    public TenantId TenantId { get; private set; } = default!;
    public string LanguageCode { get; private set; } = default!; // "tr", "en", "de"
    public bool IsDefault { get; private set; }
    public bool IsFallback { get; private set; }

    private SupportedLanguage() { } // EF Core

    public static SupportedLanguage Create(
        TenantId tenantId,
        string languageCode,
        bool isDefault = false,
        bool isFallback = false)
    {
        return new SupportedLanguage
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            LanguageCode = languageCode.ToLowerInvariant(),
            IsDefault = isDefault,
            IsFallback = isFallback
        };
    }

    public void SetAsDefault() => IsDefault = true;
    public void SetAsFallback() => IsFallback = true;
    public void ClearDefault() => IsDefault = false;
    public void ClearFallback() => IsFallback = false;
}

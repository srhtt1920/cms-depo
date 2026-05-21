namespace CMS.Application.Features.Tenants.GetTenantSettings;

public sealed record TenantSettingsDto(
    Guid TenantId,
    string Name,
    string DefaultLanguageCode,
    IReadOnlyList<SupportedLanguageDto> SupportedLanguages);

public sealed record SupportedLanguageDto(
    string LanguageCode,
    bool IsDefault,
    bool IsFallback);

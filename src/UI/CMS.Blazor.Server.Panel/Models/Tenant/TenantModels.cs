namespace CMS.Blazor.Server.Panel.Models.Tenant;

public sealed record TenantListDto(
    Guid Id, string Name, List<string> Languages, string DefaultLanguage, bool IsActive);

public sealed record TenantDetailDto(
    Guid Id, string Name,
    List<string> Languages, string DefaultLanguage,
    bool IsActive, int UserCount, int ContentCount,
    DateTime CreatedAt, DateTime? LastActivityAt,
    TenantMaintenanceDto? Maintenance,
    List<TenantUserDto> Users);

public sealed record TenantUserDto(
    Guid UserId, string Email, string? DisplayName,
    List<string> Roles, bool IsActive, DateTime? LastLogin);

public sealed record TenantMaintenanceDto(
    Guid TenantId, bool IsMaintenanceMode,
    string? MaintenanceTitle = null, string? MaintenanceMessage = null,
    string? AllowedEmailPattern = null, DateTime? PlannedEndAt = null);

public sealed record CreateTenantRequest(string Name, List<string> Languages);
public sealed record CreateTenantResponse(Guid TenantId, string Name);
public sealed record UpdateTenantNameRequest(string Name);
public sealed record UpdateLanguagesApiRequest(List<string> LanguageCodes, string DefaultLanguageCode);
public sealed record SetTenantActiveRequest(bool IsActive, string? Reason = null, string? Message = null);
public sealed record SetMaintenanceModeRequest(bool IsMaintenanceMode, string? Title = null, string? Message = null, string? AllowedEmailPattern = null, DateTime? PlannedEndAt = null);

// ── Tenant Settings (GET /api/tenants/{id}/settings) ──────────────────────
public sealed record TenantSettingsDto(
    Guid TenantId,
    string Name,
    string DefaultLanguageCode,
    List<SupportedLanguageDto> SupportedLanguages);

public sealed record SupportedLanguageDto(
    string LanguageCode,
    bool IsDefault,
    bool IsFallback);
// ── Tenant Status State Machine ────────────────────────────────────────────
public enum TenantStatus
{
    Draft,           // Oluşturuldu, provisioning başlamadı
    Provisioning,    // Default data oluşturuluyor
    ProvisionFailed, // Provisioning başarısız
    Active,          // Kullanılabilir
    Suspended,       // Geçici askıya alındı
    Archived         // Kalıcı kapatıldı
}

// ── Tenant Status Update ───────────────────────────────────────────────────
public sealed record SetTenantStatusRequest(
    TenantStatus Status,
    string? Reason = null,
    string? Message = null);

// ── Provisioning ───────────────────────────────────────────────────────────
public sealed record TenantProvisionRequest(
    string Name,
    string? Subdomain,
    List<string> Languages,
    string DefaultLanguage,
    string? AdminEmail,
    List<Guid>? AdminRoleIds,
    bool CreateDefaultContent = true,
    bool CreateDefaultNavigation = true,
    string? Plan = "starter",
    string? BrandColor = null);

public sealed record TenantProvisionResponse(
    Guid TenantId,
    string Name,
    TenantStatus Status,
    int EstimatedReadySeconds = 15);

public sealed record TenantProvisioningStatusDto(
    Guid TenantId,
    TenantStatus Status,
    List<string> CompletedSteps,
    List<string> PendingSteps,
    string? FailureReason = null);

// ── Feature Flags ──────────────────────────────────────────────────────────
public sealed record TenantFeatureFlagsDto(
    bool ApprovalWorkflow,
    bool ScheduledPublish,
    bool Multilingual,
    bool MediaLibrary,
    bool ApiAccess,
    bool Sso,
    bool CustomDomain,
    bool AdvancedSeo);

public sealed record UpdateTenantFeatureFlagsRequest(
    Dictionary<string, bool> Flags);

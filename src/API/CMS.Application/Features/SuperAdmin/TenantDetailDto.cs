namespace CMS.Application.Features.SuperAdmin;

public sealed record TenantDetailDto(
    Guid TenantId,
    string Name,
    string DefaultLanguageCode,
    bool IsActive,
    bool IsMaintenanceMode,
    int UserCount,
    DateTime CreatedAt);

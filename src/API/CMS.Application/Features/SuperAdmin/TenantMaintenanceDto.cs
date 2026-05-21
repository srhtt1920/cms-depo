namespace CMS.Application.Features.SuperAdmin;

public sealed record TenantMaintenanceDto(
    Guid TenantId,
    bool IsMaintenanceMode,
    string? MaintenanceTitle = null,
    string? MaintenanceMessage = null,
    string? AllowedEmailPattern = null,
    DateTime? PlannedEndAt = null);

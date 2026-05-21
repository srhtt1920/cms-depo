namespace CMS.Application.Features.SuperAdmin;

public sealed record SuperAdminListDto(
    Guid UserId,
    string Email,
    string? DisplayName,
    DateTime GrantedAt,
    string? GrantedByEmail = null,
    bool IsActive = true);

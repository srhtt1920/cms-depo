namespace CMS.Application.Features.SuperAdmin;


public sealed record UserTransferPreviewDto(
    Guid UserId,
    string Email,
    string? DisplayName,
    Guid SourceTenantId,
    string SourceTenantName,
    List<string> CurrentRoles,
    bool HasActiveSession = false,
    int OwnedContentCount = 0);
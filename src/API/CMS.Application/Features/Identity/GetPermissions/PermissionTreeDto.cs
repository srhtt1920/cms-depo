namespace CMS.Application.Features.Identity.GetPermissions;

public sealed record PermissionTreeDto(
    int Version,
    IReadOnlyList<PermissionNodeDto> Tree);

public sealed record PermissionNodeDto(
    string Key,
    string DisplayName,
    bool Granted,
    IReadOnlyList<PermissionNodeDto> Children);

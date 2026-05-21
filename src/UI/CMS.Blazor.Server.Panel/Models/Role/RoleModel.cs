namespace CMS.Blazor.Server.Panel.Models.Role;

public sealed record RoleListDto(
    Guid Id, string Name, List<string> PermissionKeys, int UserCount, string? Description);

public sealed record RoleDetailDto(
    Guid Id, string Name,
    List<string> PermissionKeys,
    int UserCount, bool IsActive, DateTime CreatedAt);

public sealed record CreateRoleApiRequest(string Name, List<string> PermissionKeys);
public sealed record CreateRoleApiResponse(Guid RoleId, string Name);
public sealed record UpdateRoleApiRequest(string Name, List<string> PermissionKeys);
public sealed record AssignRoleApiRequest(Guid UserId, Guid TenantId);
public sealed record RevokeRoleApiRequest(Guid UserId, Guid TenantId);

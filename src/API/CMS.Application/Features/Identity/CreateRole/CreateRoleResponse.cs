namespace CMS.Application.Features.Identity.CreateRole;

/// <summary>
/// POST /api/roles response.
/// Blazor: CreateRoleApiResponse(RoleId, Name) ile parse edilir.
/// </summary>
public sealed record CreateRoleResponse(Guid RoleId, string Name);

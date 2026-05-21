
using CMS.SharedKernel.Pagination;

namespace CMS.Application.Features.Identity.ListRoles;

public sealed record ListRolesResponse(Paginate<RoleListItem> Data);

public sealed record RoleListItem(Guid Id, string Name, List<string> PermissionKeys, int UserCount);
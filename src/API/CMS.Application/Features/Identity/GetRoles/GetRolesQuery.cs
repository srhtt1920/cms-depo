using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Identity.GetRoles;

public sealed record GetRolesQuery : IRequest<Result<List<RoleListDto>>>;

public sealed record RoleListDto(
    Guid         Id,
    string       Name,
    List<string> PermissionKeys,
    int          UserCount);

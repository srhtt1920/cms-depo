using CMS.Application.Features.Identity.GetRoles;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Identity.UpdateRole;

public sealed record UpdateRoleCommand(
    Guid         RoleId,
    string       Name,
    List<string> PermissionKeys) : IRequest<Result<RoleListDto>>;

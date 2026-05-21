using CMS.Application.Features.Identity.GetUsers;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Identity.UpdateUserRoles;

public sealed record UpdateUserRolesCommand(
    Guid       UserId,
    List<Guid> RoleIds) : IRequest<Result<UserListDto>>;

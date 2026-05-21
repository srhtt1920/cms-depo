using CMS.Domain.Identity;
using CMS.Domain.Tenants;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Identity.AssignRole;

[RequirePermission("users.manage")]
public sealed class AssignRoleHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository)
    : IRequestHandler<AssignRoleCommand, Result>
{
    public async Task<Result> Handle(AssignRoleCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetWithRolesAsync(
            UserId.From(request.UserId), ct);

        if (user is null)
            return Result.Failure(Error.NotFound("User.NotFound", "User not found."));

        var role = await roleRepository.GetByIdAsync(RoleId.From(request.RoleId), ct);

        if (role is null)
            return Result.Failure(Error.NotFound("Role.NotFound", "Role not found."));

        if (role.TenantId != TenantId.From(request.TenantId))
            return Result.Failure(Error.Forbidden("Role.WrongTenant", "Role does not belong to this tenant."));

        user.AssignRole(TenantId.From(request.TenantId), RoleId.From(request.RoleId));
        await userRepository.UpdateAsync(user, ct);

        return Result.Success();
    }
}

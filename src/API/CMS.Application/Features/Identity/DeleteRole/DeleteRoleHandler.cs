using CMS.Application.Common.Abstractions;
using CMS.Domain.Identity;
using CMS.Domain.Tenants;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Identity.DeleteRole;

[RequirePermission("roles.manage")]
public sealed class DeleteRoleHandler(
    IRoleRepository roleRepository,
    IUserRepository userRepository,
    ITenantContext  tenantContext)
    : IRequestHandler<DeleteRoleCommand, Result>
{
    public async Task<Result> Handle(DeleteRoleCommand request, CancellationToken ct)
    {
        var role = await roleRepository.GetByIdAsync(RoleId.From(request.RoleId), ct);
        if (role is null)
            return Result.Failure(Error.NotFound("Role.NotFound", "Role not found."));

        if (role.TenantId != TenantId.From(tenantContext.TenantId))
            return Result.Failure(
                Error.Forbidden("Role.WrongTenant", "Role does not belong to this tenant."));

        // Önce bu rola atanmış kullanıcılardan rolü kaldır
        var tenantId = TenantId.From(tenantContext.TenantId);
        var users = await userRepository.GetByTenantAsync(tenantId, ct);
        foreach (var user in users.Where(u =>
            u.TenantRoles.Any(utr => utr.RoleId == role.Id)))
        {
            user.RevokeRole(tenantId, role.Id);
            await userRepository.UpdateAsync(user, ct);
        }

        await roleRepository.DeleteAsync(role.Id, ct);
        return Result.Success();
    }
}

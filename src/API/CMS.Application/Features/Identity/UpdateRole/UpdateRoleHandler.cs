using CMS.Application.Common.Abstractions;
using CMS.Application.Features.Identity.GetRoles;
using CMS.Domain.Identity;
using CMS.Domain.Tenants;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Identity.UpdateRole;

[RequirePermission("roles.manage")]
public sealed class UpdateRoleHandler(
    IRoleRepository        roleRepository,
    IPermissionRepository  permissionRepository,
    IUserRepository        userRepository,
    ITenantContext         tenantContext)
    : IRequestHandler<UpdateRoleCommand, Result<RoleListDto>>
{
    public async Task<Result<RoleListDto>> Handle(
        UpdateRoleCommand request, CancellationToken ct)
    {
        var role = await roleRepository.GetWithPermissionsAsync(
            RoleId.From(request.RoleId), ct);

        if (role is null)
            return Result.Failure<RoleListDto>(
                Error.NotFound("Role.NotFound", "Role not found."));

        if (role.TenantId != TenantId.From(tenantContext.TenantId))
            return Result.Failure<RoleListDto>(
                Error.Forbidden("Role.WrongTenant", "Role does not belong to this tenant."));

        role.Rename(request.Name);
        role.ClearPermissions();

        if (request.PermissionKeys.Count > 0)
        {
            var allPerms = await permissionRepository.GetAllAsync(ct);
            foreach (var perm in allPerms.Where(p =>
                request.PermissionKeys.Contains(p.Key)))
                role.AddPermission(perm);
        }

        await roleRepository.UpdateAsync(role, ct);

        var tenantId  = TenantId.From(tenantContext.TenantId);
        var users     = await userRepository.GetByTenantAsync(tenantId, ct);
        var userCount = users.Count(u =>
            u.TenantRoles.Any(utr =>
                utr.TenantId == tenantId && utr.RoleId == role.Id));

        return new RoleListDto(
            role.Id.Value,
            role.Name,
            role.Permissions.Select(p => p.Key).ToList(),
            userCount);
    }
}

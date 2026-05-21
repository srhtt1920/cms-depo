using CMS.Application.Common.Abstractions;
using CMS.Application.Features.Identity.GetUsers;
using CMS.Domain.Identity;
using CMS.Domain.Tenants;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Identity.UpdateUserRoles;

[RequirePermission("users.manage")]
public sealed class UpdateUserRolesHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    ITenantContext  tenantContext)
    : IRequestHandler<UpdateUserRolesCommand, Result<UserListDto>>
{
    public async Task<Result<UserListDto>> Handle(
        UpdateUserRolesCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetWithRolesAsync(
            UserId.From(request.UserId), ct);

        if (user is null)
            return Result.Failure<UserListDto>(
                Error.NotFound("User.NotFound", "User not found."));

        var tenantId  = TenantId.From(tenantContext.TenantId);
        var roleNames = new List<string>();

        // Mevcut tenant rollerini temizle
        foreach (var utr in user.TenantRoles.Where(r => r.TenantId == tenantId).ToList())
            user.RevokeRole(tenantId, utr.RoleId);

        // Yeni rolleri ata
        foreach (var roleId in request.RoleIds)
        {
            var role = await roleRepository.GetByIdAsync(RoleId.From(roleId), ct);
            if (role is null || role.TenantId != tenantId) continue;
            user.AssignRole(tenantId, role.Id);
            roleNames.Add(role.Name);
        }

        await userRepository.UpdateAsync(user, ct);

        return new UserListDto(
            user.Id.Value, 
            user.Email, 
            user.DisplayName,
            roleNames,
            user.IsActive,
            null,
            user.PermissionVersion);
    }
}

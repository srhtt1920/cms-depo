using CMS.Application.Common.Abstractions;
using CMS.Domain.Identity;
using CMS.Domain.Tenants;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.SuperAdmin.AssignUserToTenant;

public sealed record AssignUserToTenantCommand(
    Guid UserId,
    Guid TenantId,
    List<Guid> RoleIds)
    : IRequest<Result>;

[RequirePermission("superadmin")]
public sealed class AssignUserToTenantHandler(
    IUserRepository userRepo,
    IRoleRepository roleRepo,
    IAuditLogger auditLogger)
    : IRequestHandler<AssignUserToTenantCommand, Result>
{
    public async Task<Result> Handle(AssignUserToTenantCommand req, CancellationToken ct)
    {
        var user = await userRepo.GetWithRolesAsync(UserId.From(req.UserId), ct);
        if (user is null)
            return Result.Failure(Error.NotFound("User.NotFound", "User not found."));

        var tenantId = TenantId.From(req.TenantId);
        var roles = await roleRepo.GetByTenantAsync(tenantId, ct);
        var roleMap = roles.ToDictionary(r => r.Id.Value);

        foreach (var roleId in req.RoleIds)
            if (roleMap.TryGetValue(roleId, out var role))
                user.AssignRole(tenantId, role.Id);

        await userRepo.UpdateAsync(user, ct);
        await auditLogger.LogAsync("User.AssignedToTenant", "User", req.UserId.ToString(),
            $"Assigned to tenant {req.TenantId}", ct);
        return Result.Success();
    }
}
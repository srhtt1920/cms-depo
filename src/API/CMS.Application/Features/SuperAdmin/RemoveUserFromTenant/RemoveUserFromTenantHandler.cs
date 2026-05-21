using CMS.Application.Common.Abstractions;
using CMS.Domain.Identity;
using CMS.Domain.Tenants;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.SuperAdmin.RemoveUserFromTenant;

public sealed record RemoveUserFromTenantCommand(Guid TenantId, Guid UserId) : IRequest<Result>;

[RequirePermission("superadmin")]
public sealed class RemoveUserFromTenantHandler(
    IUserRepository userRepo,
    IRoleRepository roleRepo,
    IAuditLogger auditLogger)
    : IRequestHandler<RemoveUserFromTenantCommand, Result>
{
    public async Task<Result> Handle(RemoveUserFromTenantCommand req, CancellationToken ct)
    {
        var user = await userRepo.GetWithRolesAsync(UserId.From(req.UserId), ct);
        if (user is null)
            return Result.Failure(Error.NotFound("User.NotFound", "User not found."));

        var tenantId = TenantId.From(req.TenantId);
        var tenantRoles = user.TenantRoles
            .Where(r => r.TenantId == tenantId)
            .ToList();

        foreach (var tr in tenantRoles)
            user.RevokeRole(tenantId, tr.RoleId);

        await userRepo.UpdateAsync(user, ct);
        await auditLogger.LogAsync("User.RemovedFromTenant", "User", req.UserId.ToString(),
            $"Removed from tenant {req.TenantId}", ct);
        return Result.Success();
    }
}

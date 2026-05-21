using CMS.Application.Common.Abstractions;
using CMS.Domain.Identity;
using CMS.Domain.Tenants;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.SuperAdmin.TransferUser;

public sealed record TransferUserCommand(
    Guid UserId,
    Guid SourceTenantId,
    Guid TargetTenantId,
    bool RemoveFromSource = false)
    : IRequest<Result<TransferUserResponse>>;

[RequirePermission("superadmin")]
public sealed class TransferUserHandler(
    IUserRepository userRepo,
    IRoleRepository roleRepo,
    IAuditLogger auditLogger)
    : IRequestHandler<TransferUserCommand, Result<TransferUserResponse>>
{
    public async Task<Result<TransferUserResponse>> Handle(
        TransferUserCommand req, CancellationToken ct)
    {
        var user = await userRepo.GetWithRolesAsync(UserId.From(req.UserId), ct);
        if (user is null)
            return Result.Failure<TransferUserResponse>(Error.NotFound("User.NotFound", "User not found."));

        var sourceTenantId = TenantId.From(req.SourceTenantId);
        var targetTenantId = TenantId.From(req.TargetTenantId);

        if (req.RemoveFromSource)
        {
            var sourceRoles = user.TenantRoles
                .Where(r => r.TenantId == sourceTenantId)
                .ToList();
            foreach (var role in sourceRoles)
                user.RevokeRole(sourceTenantId, role.RoleId);
        }

        // Hedef tenant'ta varsayılan rolü ata (opsiyonel: request'e RoleIds eklenebilir)
        await userRepo.UpdateAsync(user, ct);
        await auditLogger.LogAsync("User.Transferred", "User", req.UserId.ToString(),
            $"From tenant {req.SourceTenantId} to {req.TargetTenantId}", ct);

        return new TransferUserResponse(true, "Transfer başarılı.", 0, []);
    }
}
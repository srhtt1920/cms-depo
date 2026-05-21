using CMS.Application.Common.Abstractions;
using CMS.Domain.Tenants;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.SuperAdmin.SetTenantActive;

public sealed record SetTenantActiveCommand(Guid TenantId, bool IsActive) : IRequest<Result>;

[RequirePermission("superadmin")]
public sealed class SetTenantActiveHandler(
    ITenantRepository tenantRepo,
    IAuditLogger auditLogger)
    : IRequestHandler<SetTenantActiveCommand, Result>
{
    public async Task<Result> Handle(SetTenantActiveCommand req, CancellationToken ct)
    {
        var tenant = await tenantRepo.GetByIdAsync(TenantId.From(req.TenantId), ct);
        if (tenant is null)
            return Result.Failure(Error.NotFound("Tenant.NotFound", "Tenant not found."));

        if (req.IsActive) tenant.Activate(); else tenant.Deactivate();
        await tenantRepo.UpdateAsync(tenant, ct);
        await auditLogger.LogAsync(req.IsActive ? "Tenant.Activated" : "Tenant.Deactivated",
            "Tenant", req.TenantId.ToString(), null, ct);
        return Result.Success();
    }
}
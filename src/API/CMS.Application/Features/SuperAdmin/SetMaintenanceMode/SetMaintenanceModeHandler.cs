using CMS.Application.Common.Abstractions;
using CMS.Domain.Tenants;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.SuperAdmin.SetMaintenanceMode;

public sealed record SetMaintenanceModeCommand(Guid TenantId, bool IsMaintenanceMode) : IRequest<Result>;

[RequirePermission("superadmin")]
public sealed class SetMaintenanceModeHandler(
    ITenantRepository tenantRepo,
    IAuditLogger auditLogger)
    : IRequestHandler<SetMaintenanceModeCommand, Result>
{
    public async Task<Result> Handle(SetMaintenanceModeCommand req, CancellationToken ct)
    {
        var tenant = await tenantRepo.GetByIdAsync(TenantId.From(req.TenantId), ct);
        if (tenant is null)
            return Result.Failure(Error.NotFound("Tenant.NotFound", "Tenant not found."));

        tenant.SetMaintenanceMode(req.IsMaintenanceMode);
        await tenantRepo.UpdateAsync(tenant, ct);
        await auditLogger.LogAsync(
            req.IsMaintenanceMode ? "Tenant.MaintenanceOn" : "Tenant.MaintenanceOff",
            "Tenant", req.TenantId.ToString(), null, ct);
        return Result.Success();
    }
}

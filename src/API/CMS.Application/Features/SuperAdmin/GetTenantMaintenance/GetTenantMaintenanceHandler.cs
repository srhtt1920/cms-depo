using CMS.Domain.Tenants;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.SuperAdmin.GetTenantMaintenance;

public sealed record GetTenantMaintenanceQuery(Guid TenantId) : IRequest<Result<TenantMaintenanceDto>>;

[RequirePermission("superadmin")]
public sealed class GetTenantMaintenanceHandler(ITenantRepository tenantRepo)
    : IRequestHandler<GetTenantMaintenanceQuery, Result<TenantMaintenanceDto>>
{
    public async Task<Result<TenantMaintenanceDto>> Handle(
        GetTenantMaintenanceQuery req, CancellationToken ct)
    {
        var tenant = await tenantRepo.GetByIdAsync(TenantId.From(req.TenantId), ct);
        if (tenant is null)
            return Result.Failure<TenantMaintenanceDto>(Error.NotFound("Tenant.NotFound", "Tenant not found."));

        return new TenantMaintenanceDto(tenant.Id.Value, tenant.IsMaintenanceMode);
    }
}

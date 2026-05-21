using CMS.Domain.Tenants;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Tenants.DeactivateTenant;

[RequirePermission("tenants.manage")]
public sealed class DeactivateTenantHandler(ITenantRepository tenantRepository)
    : IRequestHandler<DeactivateTenantCommand, Result>
{
    public async Task<Result> Handle(DeactivateTenantCommand request, CancellationToken ct)
    {
        var tenant = await tenantRepository.GetByIdAsync(TenantId.From(request.TenantId), ct);
        if (tenant is null)
            return Result.Failure(Error.NotFound("Tenant.NotFound", "Tenant not found."));

        tenant.Deactivate();
        await tenantRepository.UpdateAsync(tenant, ct);
        return Result.Success();
    }
}
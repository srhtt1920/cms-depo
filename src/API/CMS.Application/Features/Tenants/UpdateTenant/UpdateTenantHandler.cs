using CMS.Domain.Tenants;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Tenants.UpdateTenant;

[RequirePermission("tenants.manage")]
public sealed class UpdateTenantHandler(ITenantRepository tenantRepository)
: IRequestHandler<UpdateTenantCommand, Result>
{
    public async Task<Result> Handle(UpdateTenantCommand request, CancellationToken ct)
    {
        var tenant = await tenantRepository.GetByIdAsync(TenantId.From(request.TenantId), ct);
        if (tenant is null)
            return Result.Failure(Error.NotFound("Tenant.NotFound", "Tenant not found."));

        //tenant.UpdateName(request.Name);
        await tenantRepository.UpdateAsync(tenant, ct);
        return Result.Success();
    }
}

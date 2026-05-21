using CMS.Domain.Tenants;
using CMS.Domain.Tenants.Errors;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Tenants.CreateTenant;

[RequirePermission("tenants.manage")]
public sealed class CreateTenantHandler(ITenantRepository tenantRepository)
    : IRequestHandler<CreateTenantCommand, Result<CreateTenantResponse>>
{
    public async Task<Result<CreateTenantResponse>> Handle(
        CreateTenantCommand request, CancellationToken ct)
    {
        var exists = await tenantRepository.ExistsByNameAsync(request.Name, ct);

        if (exists)
            return Result.Failure<CreateTenantResponse>(
                TenantErrors.NameAlreadyExists(request.Name));

        var tenant = Tenant.CreateBusiness(request.Name, request.DefaultLanguageCode);
        await tenantRepository.AddAsync(tenant, ct);
        return new CreateTenantResponse(tenant.Id.Value, tenant.Name);
    }
}

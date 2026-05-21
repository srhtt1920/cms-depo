using CMS.Domain.Tenants;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Tenants.GetTenants;

[RequirePermission("tenants.manage")]
public sealed class GetTenantsHandler(ITenantRepository tenantRepository)
    : IRequestHandler<GetTenantsQuery, Result<List<TenantListDto>>>
{
    public async Task<Result<List<TenantListDto>>> Handle(
        GetTenantsQuery request, CancellationToken ct)
    {
        var tenants = await tenantRepository.GetAllAsync(ct);

        return tenants
            .Where(t => t.IsActive)
            .Select(t => new TenantListDto(
                t.Id.Value,
                t.Name,
                t.SupportedLanguages.Select(l => l.LanguageCode).ToList(),
                t.DefaultLanguageCode,
                t.IsActive))
            .ToList();
    }
}

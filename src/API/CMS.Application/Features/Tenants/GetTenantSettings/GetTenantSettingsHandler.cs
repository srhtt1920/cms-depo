using CMS.Domain.Tenants;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Tenants.GetTenantSettings;

public sealed class GetTenantSettingsHandler(ITenantRepository tenantRepository)
    : IRequestHandler<GetTenantSettingsQuery, Result<TenantSettingsDto>>
{
    public async Task<Result<TenantSettingsDto>> Handle(
        GetTenantSettingsQuery request, CancellationToken ct)
    {
        var tenant = await tenantRepository.GetByIdAsync(
            TenantId.From(request.TenantId), ct);

        if (tenant is null)
            return Result.Failure<TenantSettingsDto>(
                Error.NotFound("Tenant.NotFound", "Tenant not found."));

        return new TenantSettingsDto(
            tenant.Id.Value,
            tenant.Name,
            tenant.DefaultLanguageCode,
            tenant.SupportedLanguages
                .Select(l => new SupportedLanguageDto(l.LanguageCode, l.IsDefault, l.IsFallback))
                .ToList());
    }
}

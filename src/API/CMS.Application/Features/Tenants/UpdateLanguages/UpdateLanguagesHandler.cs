using CMS.Domain.Tenants;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Tenants.UpdateLanguages;

[RequirePermission("tenants.manage")]
public sealed class UpdateLanguagesHandler(ITenantRepository tenantRepository)
    : IRequestHandler<UpdateLanguagesCommand, Result>
{
    public async Task<Result> Handle(UpdateLanguagesCommand request, CancellationToken ct)
    {
        var tenant = await tenantRepository.GetByIdAsync(
            TenantId.From(request.TenantId), ct);
        if (tenant is null)
            return Result.Failure(Error.NotFound("Tenant.NotFound", "Tenant not found."));

        try
        {
            tenant.SetSupportedLanguages(request.LanguageCodes, request.DefaultLanguageCode);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Validation("Tenant.InvalidLanguage", ex.Message));
        }

        await tenantRepository.UpdateAsync(tenant, ct);
        return Result.Success();
    }
}

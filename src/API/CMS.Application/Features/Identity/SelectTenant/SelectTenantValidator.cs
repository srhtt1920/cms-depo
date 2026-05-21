using FluentValidation;

namespace CMS.Application.Features.Identity.SelectTenant;

public sealed class SelectTenantValidator : AbstractValidator<SelectTenantCommand>
{
    public SelectTenantValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
    }
}

using FluentValidation;

namespace CMS.Application.Features.Tenants.CreateTenant;

public sealed class CreateTenantValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DefaultLanguageCode).NotEmpty().Length(2, 5);
    }
}

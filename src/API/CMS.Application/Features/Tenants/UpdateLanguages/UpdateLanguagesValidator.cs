using FluentValidation;

namespace CMS.Application.Features.Tenants.UpdateLanguages;

public sealed class UpdateLanguagesValidator : AbstractValidator<UpdateLanguagesCommand>
{
    public UpdateLanguagesValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.LanguageCodes).NotEmpty();
        RuleFor(x => x.DefaultLanguageCode).NotEmpty().Length(2, 5);
        RuleForEach(x => x.LanguageCodes).NotEmpty().Length(2, 5);
    }
}

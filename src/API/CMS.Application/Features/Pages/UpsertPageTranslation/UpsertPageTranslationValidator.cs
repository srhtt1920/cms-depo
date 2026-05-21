using FluentValidation;

namespace CMS.Application.Features.Pages.UpsertPageTranslation;

public sealed class UpsertPageTranslationValidator : AbstractValidator<UpsertPageTranslationCommand>
{
    public UpsertPageTranslationValidator()
    {
        RuleFor(x => x.LanguageCode).NotEmpty().Length(2, 5);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(500);
        RuleFor(x => x.LinkName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug)
            .NotEmpty()
            .MaximumLength(500)
            .Matches(@"^[a-z0-9]+(?:[/-][a-z0-9]+)*$")
            .WithMessage("Slug can only contain lowercase letters, numbers, hyphens and slashes.");
        RuleFor(x => x.MetaTitle).MaximumLength(200);
        RuleFor(x => x.MetaDescription).MaximumLength(500);
        RuleFor(x => x.MetaKeywords).MaximumLength(300);
    }
}
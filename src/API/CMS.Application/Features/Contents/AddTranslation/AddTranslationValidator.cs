using FluentValidation;

namespace CMS.Application.Features.Contents.AddTranslation;

public sealed class AddTranslationValidator : AbstractValidator<AddTranslationCommand>
{
    public AddTranslationValidator()
    {
        RuleFor(x => x.ContentId).NotEmpty();
        RuleFor(x => x.LanguageCode).NotEmpty().Length(2, 5);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Body).NotEmpty();
    }
}

using FluentValidation;

namespace CMS.Application.Features.Contents.CreateContent;

public sealed class CreateContentValidator : AbstractValidator<CreateContentCommand>
{
    public CreateContentValidator()
    {
        RuleFor(x => x.Slug)
            .NotEmpty()
            .MaximumLength(200)
            .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage("Slug must contain only lowercase letters, numbers, and hyphens.");

        RuleFor(x => x.LanguageCode)
            .NotEmpty()
            .Length(2, 5);

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.Body)
            .NotEmpty();
    }
}

using CMS.Domain.Pages;
using FluentValidation;

namespace CMS.Application.Features.Pages.CreatePage;

public sealed class CreatePageValidator : AbstractValidator<CreatePageCommand>
{
    public CreatePageValidator()
    {
        RuleFor(x => x.LanguageCode)
            .NotEmpty()
            .Length(2, 5);

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.LinkName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Slug)
            .NotEmpty()
            .MaximumLength(500)
            .Matches(@"^[a-z0-9]+(?:[/-][a-z0-9]+)*$")
            .WithMessage("Slug can only contain lowercase letters, numbers, hyphens and forward slashes.");

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(0);

        // Normal sayfa → ContentType zorunlu
        RuleFor(x => x.ContentType)
            .NotNull()
            .When(x => x.PageType == PageType.Default)
            .WithMessage("ContentType is required for Normal page type.");

        // Kategori → ContentType olmamalı
        RuleFor(x => x.ContentType)
            .Null()
            .When(x => x.PageType == PageType.Category)
            .WithMessage("Category pages must not have a ContentType.");

        RuleFor(x => x.ExternalUrl)
            .MaximumLength(2000)
            .Must(url => url == null || Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("ExternalUrl must be a valid absolute URL.")
            .When(x => x.ExternalUrl != null);

        RuleFor(x => x.MetaTitle).MaximumLength(200);
        RuleFor(x => x.MetaDescription).MaximumLength(500);
        RuleFor(x => x.MetaKeywords).MaximumLength(300);
    }
}
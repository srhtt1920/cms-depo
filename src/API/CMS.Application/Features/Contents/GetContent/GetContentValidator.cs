using FluentValidation;

namespace CMS.Application.Features.Contents.GetContent;

public sealed class GetContentValidator : AbstractValidator<GetContentQuery>
{
    public GetContentValidator()
    {
        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug is required.")
            .MaximumLength(200).WithMessage("Slug cannot exceed 200 characters.");
    }
}

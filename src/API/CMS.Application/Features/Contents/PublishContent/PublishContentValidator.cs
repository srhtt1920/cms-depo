using FluentValidation;

namespace CMS.Application.Features.Contents.PublishContent;

public sealed class PublishContentValidator : AbstractValidator<PublishContentCommand>
{
    public PublishContentValidator()
    {
        RuleFor(x => x.ContentId).NotEmpty();
    }
}

using FluentValidation;

namespace CMS.Application.Features.Pages.MovePage;

public sealed class MovePageValidator : AbstractValidator<MovePageCommand>
{
    public MovePageValidator()
    {
        RuleFor(x => x.NewOrder).GreaterThanOrEqualTo(0);

        RuleFor(x => x.NewParentId)
            .Must((cmd, parentId) => parentId != cmd.PageId)
            .WithMessage("A page cannot be its own parent.");
    }
}

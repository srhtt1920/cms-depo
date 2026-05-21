using FluentValidation;

namespace CMS.Application.Features.Identity.CreateRole;

public sealed class CreateRoleValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

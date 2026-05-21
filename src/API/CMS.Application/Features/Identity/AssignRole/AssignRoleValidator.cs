using FluentValidation;

namespace CMS.Application.Features.Identity.AssignRole;

public sealed class AssignRoleValidator : AbstractValidator<AssignRoleCommand>
{
    public AssignRoleValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RoleId).NotEmpty();
    }
}

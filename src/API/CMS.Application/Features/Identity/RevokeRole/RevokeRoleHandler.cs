using CMS.Domain.Identity;
using CMS.Domain.Tenants;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Identity.RevokeRole;

[RequirePermission("users.manage")]
public sealed class RevokeRoleHandler(IUserRepository userRepository)
    : IRequestHandler<RevokeRoleCommand, Result>
{
    public async Task<Result> Handle(RevokeRoleCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetWithRolesAsync(
            UserId.From(request.UserId), ct);

        if (user is null)
            return Result.Failure(Error.NotFound("User.NotFound", "User not found."));

        user.RevokeRole(TenantId.From(request.TenantId), RoleId.From(request.RoleId));
        await userRepository.UpdateAsync(user, ct);
        return Result.Success();
    }
}

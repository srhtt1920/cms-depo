using CMS.Application.Common.Abstractions;
using CMS.Domain.Identity;
using CMS.Domain.Tenants;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Identity.DeleteUser;

[RequirePermission("users.manage")]
public sealed class DeleteUserHandler(
   IUserRepository userRepository,
   ITenantContext tenantContext)
   : IRequestHandler<DeleteUserCommand, Result>
{
    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetWithRolesAsync(UserId.From(request.UserId), ct);
        if (user is null)
            return Result.Failure(Error.NotFound("User.NotFound", "User not found."));

        // Kullanıcıyı tenant'tan çıkar (tüm rolleri sil)
        var tenantId = TenantId.From(tenantContext.TenantId);
        var tenantRoles = user.TenantRoles.Where(r => r.TenantId == tenantId).ToList();
        foreach (var tr in tenantRoles)
            user.RevokeRole(tenantId, tr.RoleId);

        // Deaktive et
        user.Deactivate();
        await userRepository.UpdateAsync(user, ct);
        return Result.Success();
    }
}

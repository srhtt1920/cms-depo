using CMS.Application.Common.Abstractions;
using CMS.Application.Features.Identity.GetUsers;
using CMS.Domain.Identity;
using CMS.Domain.Tenants;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Identity.GetUserById;

[RequirePermission("users.manage")]
public sealed class GetUserByIdHandler(
  IUserRepository userRepository,
  IRoleRepository roleRepository,
  ITenantContext tenantContext)
  : IRequestHandler<GetUserByIdQuery, Result<UserListDto>>
{
    public async Task<Result<UserListDto>> Handle(
        GetUserByIdQuery request, CancellationToken ct)
    {
        var user = await userRepository.GetWithRolesAsync(UserId.From(request.UserId), ct);
        if (user is null)
            return Result.Failure<UserListDto>(
                Error.NotFound("User.NotFound", "User not found."));

        var tenantId = TenantId.From(tenantContext.TenantId);
        var roles = await roleRepository.GetByTenantAsync(tenantId, ct);
        var roleMap = roles.ToDictionary(r => r.Id, r => r.Name);

        return new UserListDto(
            user.Id.Value, user.Email, user.DisplayName,
            user.TenantRoles
                .Where(r => r.TenantId == tenantId)
                .Select(r => roleMap.TryGetValue(r.RoleId, out var name) ? name : "")
                .Where(n => !string.IsNullOrEmpty(n)).ToList(),
            user.IsActive, user.LastLoginAt, user.PermissionVersion);
    }
}

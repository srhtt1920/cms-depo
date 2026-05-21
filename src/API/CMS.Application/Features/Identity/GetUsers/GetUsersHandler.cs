using CMS.Application.Common.Abstractions;
using CMS.Domain.Identity;
using CMS.Domain.Tenants;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Identity.GetUsers;

[RequirePermission("users.manage")]
public sealed class GetUsersHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    ITenantContext  tenantContext)
    : IRequestHandler<GetUsersQuery, Result<List<UserListDto>>>
{
    public async Task<Result<List<UserListDto>>> Handle(
        GetUsersQuery request, CancellationToken ct)
    {
        var tenantId = TenantId.From(tenantContext.TenantId);
        var users = await userRepository.GetByTenantAsync(tenantId, ct);
        var roles = await roleRepository.GetByTenantAsync(tenantId, ct);
        var roleMap = roles.ToDictionary(r => r.Id, r => r.Name);

        var result = users.Select(u => new UserListDto(
            u.Id.Value,
            u.Email,   
            u.DisplayName,
            u.TenantRoles
                .Where(r => r.TenantId == tenantId)
                .Select(r => roleMap.TryGetValue(r.RoleId, out var name) ? name : "")
                .Where(n => !string.IsNullOrEmpty(n))
                .ToList(),
            u.IsActive,
            null,                   // LastLogin: activity log'dan gelecek
            u.PermissionVersion))
            .ToList();

        return result;
    }
}

using CMS.Application.Common.Abstractions;
using CMS.Domain.Identity;
using CMS.Domain.Tenants;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Identity.GetRoles;

[RequirePermission("roles.manage")]
public sealed class GetRolesHandler(
    IRoleRepository roleRepository,
    IUserRepository userRepository,
    ITenantContext  tenantContext)
    : IRequestHandler<GetRolesQuery, Result<List<RoleListDto>>>
{
    public async Task<Result<List<RoleListDto>>> Handle(
        GetRolesQuery request, CancellationToken ct)
    {
        var tenantId = TenantId.From(tenantContext.TenantId);

        // İki sorgu — N+1 yok
        var roles = await roleRepository.GetByTenantAsync(tenantId, ct);
        var users = await userRepository.GetByTenantAsync(tenantId, ct);

        // Rol→kullanıcı sayısı bir kez hesapla
        var roleUserCount = users
            .SelectMany(u => u.TenantRoles.Where(utr => utr.TenantId == tenantId))
            .GroupBy(utr => utr.RoleId)
            .ToDictionary(g => g.Key, g => g.Count());

        var result = roles.Select(r => new RoleListDto(
            r.Id.Value,
            r.Name,
            r.Permissions.Select(p => p.Key).ToList(),
            roleUserCount.GetValueOrDefault(r.Id, 0)))
            .ToList();

        return result;
    }
}

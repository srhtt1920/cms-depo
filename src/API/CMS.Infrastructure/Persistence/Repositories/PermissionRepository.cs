using CMS.Domain.Identity;
using CMS.Domain.Tenants;
using Microsoft.EntityFrameworkCore;

namespace CMS.Infrastructure.Persistence.Repositories;

public sealed class PermissionRepository(CmsDbContext context) : IPermissionRepository
{
    public async Task<IReadOnlyList<Permission>> GetAllAsync(CancellationToken ct = default) =>
        await context.Permissions.ToListAsync(ct);

    public async Task<IReadOnlyList<string>> GetKeysByUserAndTenantAsync(
        UserId userId, TenantId tenantId, CancellationToken ct = default)
    {
        // UserTenantRole → Role → Permission.Key join
        return await context.UserTenantRoles
            .Where(utr => utr.UserId == userId && utr.TenantId == tenantId)
            .Join(
                context.Roles
                    .Include(r => r.Permissions)
                    .IgnoreQueryFilters(),
                utr => utr.RoleId,
                r => r.Id,
                (utr, r) => r.Permissions)
            .SelectMany(perms => perms.Select(p => p.Key))
            .Distinct()
            .ToListAsync(ct);
    }
}

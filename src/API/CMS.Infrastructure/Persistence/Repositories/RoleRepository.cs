using CMS.Domain.Identity;
using CMS.Domain.Tenants;
using Microsoft.EntityFrameworkCore;

namespace CMS.Infrastructure.Persistence.Repositories;

public sealed class RoleRepository(CmsDbContext context) : IRoleRepository
{
    public async Task<Role?> GetByIdAsync(RoleId id, CancellationToken ct = default) =>
        await context.Roles.FirstOrDefaultAsync(r => r.Id == id && r.IsActive, ct);

    public async Task<IReadOnlyList<Role>> GetByTenantAsync(TenantId tenantId, CancellationToken ct = default) =>
        await context.Roles
            .Where(r => r.TenantId == tenantId && r.IsActive)
            .ToListAsync(ct);

    public async Task<Role?> GetWithPermissionsAsync(RoleId id, CancellationToken ct = default) =>
        await context.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == id && r.IsActive, ct);

    public async Task<Role> AddAsync(Role role, CancellationToken ct = default)
    {
        await context.Roles.AddAsync(role, ct);
        await context.SaveChangesAsync(ct);
        return role;
    }

    public async Task<Role> UpdateAsync(Role role, CancellationToken ct = default)
    {
        context.Roles.Update(role);
        await context.SaveChangesAsync(ct);
        return role;
    }

    public async Task DeleteAsync(RoleId id, CancellationToken ct = default)
    {
        var role = await context.Roles.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (role is null) return;
        role.Deactivate();
        context.Roles.Update(role);
        await context.SaveChangesAsync(ct);
    }
}

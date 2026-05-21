using CMS.Domain.Identity;
using CMS.Domain.Tenants;
using Microsoft.EntityFrameworkCore;

namespace CMS.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(CmsDbContext context) : IUserRepository
{
    public async Task<User?> GetByIdAsync(UserId id, CancellationToken ct = default) =>
        await context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        await context.Users
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), ct);

    public async Task<User?> GetWithRolesAsync(UserId id, CancellationToken ct = default) =>
        await context.Users
            .Include(u => u.TenantRoles)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<IReadOnlyList<User>> GetByTenantAsync(
        TenantId tenantId, CancellationToken ct = default) =>
        await context.Users
            .Include(u => u.TenantRoles)
            .Where(u => u.TenantRoles.Any(r => r.TenantId == tenantId))
            .ToListAsync(ct);

    public async Task<User> AddAsync(User user, CancellationToken ct = default)
    {
        await context.Users.AddAsync(user, ct);
        await context.SaveChangesAsync(ct);
        return user;
    }

    public async Task<User> UpdateAsync(User user, CancellationToken ct = default)
    {
        var entry = context.Entry(user);

        if (entry.Collection(u => u.TenantRoles).IsLoaded)
            SyncTenantRoles(user);

        await context.SaveChangesAsync(ct);
        return user;
    }
    private void SyncTenantRoles(User user)
    {
        var localSnapshot = context.Set<UserTenantRole>()
            .Local
            .Where(r => r.UserId == user.Id)
            .ToList();

        var desired = user.TenantRoles.ToList();

        var toDelete = localSnapshot
            .Where(snap => !desired.Any(d =>
                d.TenantId == snap.TenantId &&
                d.RoleId == snap.RoleId))
            .ToList();

        var toAdd = desired
            .Where(d => !localSnapshot.Any(snap =>
                snap.TenantId == d.TenantId &&
                snap.RoleId == d.RoleId))
            .ToList();

        if (toDelete.Count > 0)
            context.Set<UserTenantRole>().RemoveRange(toDelete);

        if (toAdd.Count > 0)
            context.Set<UserTenantRole>().AddRange(toAdd);
    }

    public async Task<IReadOnlyList<User>> GetSuperAdminsAsync(CancellationToken ct = default)
    {
        return await context.Users
                   .Include(u => u.TenantRoles)
                   .Where(u => u.IsSuperAdmin && u.IsActive)
                   .ToListAsync(ct);
    }
}

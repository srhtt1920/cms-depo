using CMS.Domain.Tenants;
using Microsoft.EntityFrameworkCore;

namespace CMS.Infrastructure.Persistence.Repositories;

public sealed class TenantRepository(CmsDbContext context) : ITenantRepository
{
    public async Task<Tenant?> GetByIdAsync(TenantId id, CancellationToken ct = default) =>
        await context.Tenants
            .Include(t => t.SupportedLanguages)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken ct = default) =>
        await context.Tenants
            .Include(t => t.SupportedLanguages)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<SupportedLanguage>> GetSupportedLanguagesAsync(
        TenantId id, CancellationToken ct = default) =>
        await context.SupportedLanguages
            .Where(l => l.TenantId == id)
            .ToListAsync(ct);

    public async Task<Tenant> AddAsync(Tenant tenant, CancellationToken ct = default)
    {
        await context.Tenants.AddAsync(tenant, ct);
        await context.SaveChangesAsync(ct);
        return tenant;
    }

    public async Task<Tenant> UpdateAsync(Tenant tenant, CancellationToken ct = default)
    {
        context.Tenants.Update(tenant);
        await context.SaveChangesAsync(ct);
        return tenant;
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default) =>
        await context.Tenants
            .AnyAsync(t => t.Name == name, ct);
}

using CMS.Domain.Common;
using CMS.Domain.Tenants;
using CMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CMS.Infrastructure.Identity;

public sealed class TenantPolicyService(
    CmsDbContext db,
    IMemoryCache cache) : ITenantPolicyService
{
    private const string SystemTenantCacheKey = "system_tenant_id";

    public IReadOnlyList<string> AllowedSystemRoles { get; } =
        ["SuperAdmin", "SystemOperator", "PlatformAdmin"];

    public bool IsAllowedForSystemTenant(string roleName) =>
        AllowedSystemRoles.Contains(roleName, StringComparer.OrdinalIgnoreCase);

    public async Task<Guid> GetSystemTenantIdAsync(CancellationToken ct = default)
    {
        if (cache.TryGetValue(SystemTenantCacheKey, out Guid cached))
            return cached;

        var system = await db.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Type == TenantType.System, ct)
            ?? throw new InvalidOperationException("System tenant bulunamadı.");

        cache.Set(SystemTenantCacheKey, system.Id.Value,
            TimeSpan.FromHours(24)); // Root değişmez

        return system.Id.Value;
    }

    public async Task<bool> IsSystemTenantAsync(Guid tenantId, CancellationToken ct = default)
    {
        var rootId = await GetSystemTenantIdAsync(ct);
        return tenantId == rootId;
    }
}

using CMS.Application.Common.Abstractions;
using CMS.Domain.Identity;
using CMS.Domain.Tenants;

namespace CMS.Infrastructure.Caching;

public sealed class PermissionCacheService(
    ICacheService cache,
    IPermissionRepository permissionRepository,
    ICurrentUser currentUser)
    : IPermissionService
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    private static string PermSetKey(Guid userId, Guid tenantId) => $"perm:keys:{userId}:{tenantId}";
    private static string PermVersionKey(Guid userId, Guid tenantId) => $"perm:version:{userId}:{tenantId}";

    public async Task<bool> HasPermissionAsync(
        Guid userId, Guid tenantId, string permissionKey, CancellationToken ct = default)
    {
        var jwtVersion = currentUser.PermissionVersion;
        var cachedVersion = await cache.GetIntAsync(PermVersionKey(userId, tenantId), ct);

        if (cachedVersion is null || cachedVersion != jwtVersion)
            await RefreshAsync(userId, tenantId, jwtVersion, ct);

        return await cache.SetContainsAsync(PermSetKey(userId, tenantId), permissionKey, ct);
    }

    public async Task InvalidateAsync(Guid userId, Guid tenantId, CancellationToken ct = default)
    {
        await cache.RemoveAsync(PermSetKey(userId, tenantId), ct);
        await cache.RemoveAsync(PermVersionKey(userId, tenantId), ct);
    }

    private async Task RefreshAsync(Guid userId, Guid tenantId, int newVersion, CancellationToken ct)
    {
        var keys = await permissionRepository.GetKeysByUserAndTenantAsync(
            UserId.From(userId), TenantId.From(tenantId), ct);

        await cache.SetAddManyAsync(PermSetKey(userId, tenantId), keys, CacheTtl, ct);
        await cache.SetIntAsync(PermVersionKey(userId, tenantId), newVersion, CacheTtl, ct);
    }
}

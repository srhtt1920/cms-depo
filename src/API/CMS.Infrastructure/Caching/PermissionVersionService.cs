using CMS.Application.Common.Abstractions;
using CMS.Infrastructure.SignalR.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CMS.Infrastructure.Caching;

public sealed class PermissionVersionService(
    ICacheService cache,
    IHubContext<PermissionHub, IPermissionClient> hubContext)
{
    private static string VersionKey(Guid userId, Guid tenantId) =>
        $"perm:version:{userId}:{tenantId}";

    public async Task SetVersionAndNotifyAsync(
        Guid userId, Guid tenantId, int newVersion, CancellationToken ct = default)
    {
        await cache.SetIntAsync(VersionKey(userId, tenantId), newVersion, ct: ct);

        await hubContext.Clients
            .Group($"user:{userId}")
            .OnPermissionVersionChanged(userId, tenantId, newVersion);
    }

    public async Task<int?> GetCurrentVersionAsync(Guid userId, Guid tenantId) =>
        await cache.GetIntAsync(VersionKey(userId, tenantId));
}

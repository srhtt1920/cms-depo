using CMS.Domain.Identity.Events;
using CMS.Infrastructure.Caching;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CMS.Infrastructure.DomainEventHandlers;

/// <summary>
/// Kullanıcı rol ataması/kaldırması sonrası:
/// 1. Redis'teki PermissionVersion güncellenir.
/// 2. SignalR ile panel client'ı bildirilir.
/// 3. Permission key cache'i temizlenir (bir sonraki istekte yenilenir).
/// </summary>
public sealed class PermissionVersionIncrementedHandler(
    PermissionVersionService versionService,
    PermissionCacheService cacheService,
    ILogger<PermissionVersionIncrementedHandler> logger)
    : INotificationHandler<PermissionVersionIncrementedEvent>
{
    public async Task Handle(PermissionVersionIncrementedEvent notification, CancellationToken ct)
    {
        // Permission key cache'ini temizle
        await cacheService.InvalidateAsync(
            notification.UserId.Value,
            notification.TenantId.Value,
            ct);

        // Yeni version'ı Redis'e yaz + SignalR notify
        await versionService.SetVersionAndNotifyAsync(
            notification.UserId.Value,
            notification.TenantId.Value,
            notification.NewVersion,
            ct);

        logger.LogInformation(
            "Permission version updated: User {UserId} | Tenant {TenantId} | v{Version}",
            notification.UserId.Value,
            notification.TenantId.Value,
            notification.NewVersion);
    }
}

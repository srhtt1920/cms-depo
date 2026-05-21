using CMS.Domain.Tenants.Events;
using CMS.Infrastructure.Caching;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CMS.Infrastructure.DomainEventHandlers;

/// <summary>
/// Tenant dil konfigürasyonu değişince language cache'i temizler.
/// </summary>
public sealed class TenantLanguageUpdatedHandler(
    LanguageCacheService cacheService,
    ILogger<TenantLanguageUpdatedHandler> logger)
    : INotificationHandler<TenantLanguageUpdatedEvent>
{
    public async Task Handle(TenantLanguageUpdatedEvent notification, CancellationToken ct)
    {
        await cacheService.InvalidateAsync(notification.TenantId.Value);

        logger.LogInformation(
            "Language cache invalidated for Tenant {TenantId}",
            notification.TenantId.Value);
    }
}

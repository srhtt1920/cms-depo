using CMS.Domain.Contents;
using CMS.Domain.Contents.Events;
using CMS.Infrastructure.Publishing;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CMS.Infrastructure.DomainEventHandlers;

public sealed class ContentPublishedEventHandler(
    IContentRepository repo,
    IPublishPipeline   pipeline,
    ILogger<ContentPublishedEventHandler> logger)
    : INotificationHandler<ContentPublishedEvent>
{
    public async Task Handle(ContentPublishedEvent notification, CancellationToken ct)
    {
        var content = await repo.GetByIdAsync(notification.ContentId, ct);
        if (content is null)
        {
            logger.LogWarning("ContentPublishedEvent: content {Id} not found.",
                notification.ContentId.Value);
            return;
        }
        await pipeline.OnPublishedAsync(content, ct);
    }
}

using CMS.Domain.Contents;
using Microsoft.Extensions.Logging;

namespace CMS.Infrastructure.Publishing;

/// <summary>
/// İçerik yayınlandığında/kaldırıldığında sitemap cache'ini geçersiz kılar.
/// Gerçek implementasyonda CDN purge veya cache key delete yapılır.
/// </summary>
public sealed class SitemapRefreshPipelineHandler(
    ILogger<SitemapRefreshPipelineHandler> logger) : IPublishPipelineHandler
{
    public Task OnPublishedAsync(Content content, CancellationToken ct)
    {
        logger.LogInformation(
            "Sitemap refresh triggered for content {Slug} (tenant {TenantId})",
            content.Slug.Value, content.TenantId.Value);
        // TODO: Cache key sil veya CDN purge endpoint çağır
        // await _cacheService.RemoveAsync($"sitemap:{content.TenantId.Value}", ct);
        return Task.CompletedTask;
    }

    public Task OnUnpublishedAsync(Content content, CancellationToken ct)
    {
        logger.LogInformation(
            "Sitemap refresh triggered (unpublish) for content {Slug}", content.Slug.Value);
        return Task.CompletedTask;
    }
}

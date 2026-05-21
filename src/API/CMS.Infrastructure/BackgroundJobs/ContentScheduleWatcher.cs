using CMS.Domain.Contents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CMS.Infrastructure.BackgroundJobs;

// <summary>
/// Her dakika çalışır:
/// 1. Scheduled → Published : PublishAt geldi, yayınla.
/// 2. Published → Expired   : UnpublishAt geçti, süresi doldu.
///
/// BUG FIX: Her item ayrı try-catch içinde işleniyor.
/// Bir item'da hata olursa diğerleri etkilenmiyor.
/// </summary>
public sealed class ContentScheduleWatcher(
    IServiceScopeFactory scopeFactory,
    ILogger<ContentScheduleWatcher> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            try
            {
                await TickAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "ContentScheduleWatcher tick failed.");
            }
        }
    }

    private async Task TickAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IContentRepository>();
        var now = DateTime.UtcNow;

        await PublishScheduledAsync(repo, now, ct);
        await ExpirePublishedAsync(repo, now, ct);
    }

    // ── Scheduled → Published ─────────────────────────────────────────────

    private async Task PublishScheduledAsync(
        IContentRepository repo, DateTime utcNow, CancellationToken ct)
    {
        IReadOnlyList<Content> toPublish;
        try
        {
            toPublish = await repo.GetScheduledToPublishAsync(utcNow, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to query scheduled-to-publish contents.");
            return;
        }

        foreach (var content in toPublish)
        {
            // BUG FIX: her item ayrı try-catch — bir item fail ederse batch devam eder
            try
            {
                content.Publish();
                await repo.UpdateAsync(content, ct);
                logger.LogInformation(
                    "Auto-published content {Slug} (Id={Id}).",
                    content.Slug.Value, content.Id.Value);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Failed to auto-publish content {Slug} (Id={Id}).",
                    content.Slug.Value, content.Id.Value);
            }
        }
    }

    // ── Published → Expired ───────────────────────────────────────────────

    private async Task ExpirePublishedAsync(
        IContentRepository repo, DateTime utcNow, CancellationToken ct)
    {
        IReadOnlyList<Content> toExpire;
        try
        {
            toExpire = await repo.GetScheduledToExpireAsync(utcNow, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to query scheduled-to-expire contents.");
            return;
        }

        foreach (var content in toExpire)
        {
            try
            {
                content.Expire();
                await repo.UpdateAsync(content, ct);
                logger.LogInformation(
                    "Auto-expired content {Slug} (Id={Id}).",
                    content.Slug.Value, content.Id.Value);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Failed to auto-expire content {Slug} (Id={Id}).",
                    content.Slug.Value, content.Id.Value);
            }
        }
    }
}

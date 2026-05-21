using CMS.Domain.Contents;
using CMS.Domain.Pages;
using Microsoft.EntityFrameworkCore;

namespace CMS.Infrastructure.Persistence.Repositories;

public sealed class ContentLockRepository(CmsDbContext context) : IContentLockRepository
{
    // ── Queries ───────────────────────────────────────────────────────────

    public async Task<ContentLock?> GetByContentIdAsync(
        ContentId contentId, CancellationToken ct = default) =>
        await context.Set<ContentLock>()
            .FirstOrDefaultAsync(l => l.ContentId == contentId, ct);

    public async Task<ContentLock?> GetByPageIdAsync(
        PageId pageId, CancellationToken ct = default) =>
        await context.Set<ContentLock>()
            .FirstOrDefaultAsync(l => l.PageId == pageId, ct);

    public async Task<IReadOnlyList<ContentLock>> GetByContentIdsAsync(
        IEnumerable<ContentId> ids, CancellationToken ct = default)
    {
        var idList = ids.ToList();
        return await context.Set<ContentLock>()
            .Where(l => l.ContentId != null && idList.Contains(l.ContentId))
            .ToListAsync(ct);
    }

    // ── Commands ──────────────────────────────────────────────────────────

    public async Task UpsertAsync(ContentLock lockEntry, CancellationToken ct = default)
    {
        ContentLock? existing = null;

        if (lockEntry.ContentId is not null)
            existing = await GetByContentIdAsync(lockEntry.ContentId, ct);
        else if (lockEntry.PageId is not null)
            existing = await GetByPageIdAsync(lockEntry.PageId, ct);

        if (existing is not null)
        {
            context.Set<ContentLock>().Remove(existing);
            await context.SaveChangesAsync(ct);
        }

        await context.Set<ContentLock>().AddAsync(lockEntry, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task BulkUpsertAsync(
        IEnumerable<ContentLock> locks, CancellationToken ct = default)
    {
        var lockList = locks.ToList();
        var contentIds = lockList
            .Where(l => l.ContentId is not null)
            .Select(l => l.ContentId!)
            .ToList();

        if (contentIds.Count > 0)
        {
            var existing = await context.Set<ContentLock>()
                .Where(l => l.ContentId != null && contentIds.Contains(l.ContentId))
                .ToListAsync(ct);

            if (existing.Count > 0)
                context.Set<ContentLock>().RemoveRange(existing);
        }

        await context.Set<ContentLock>().AddRangeAsync(lockList, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteByContentIdAsync(
        ContentId contentId, CancellationToken ct = default)
    {
        var existing = await GetByContentIdAsync(contentId, ct);
        if (existing is null) return;
        context.Set<ContentLock>().Remove(existing);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteByPageIdAsync(
        PageId pageId, CancellationToken ct = default)
    {
        var existing = await GetByPageIdAsync(pageId, ct);
        if (existing is null) return;
        context.Set<ContentLock>().Remove(existing);
        await context.SaveChangesAsync(ct);
    }
}

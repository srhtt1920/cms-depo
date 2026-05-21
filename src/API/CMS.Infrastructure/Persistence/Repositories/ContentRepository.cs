using CMS.Domain.Contents;
using CMS.Domain.Tenants;
using CMS.SharedKernel.Pagination;
using Microsoft.EntityFrameworkCore;

namespace CMS.Infrastructure.Persistence.Repositories;

public sealed class ContentRepository(CmsDbContext context) : IContentRepository
{
    public async Task<Content?> GetByIdAsync(ContentId id, CancellationToken ct = default) =>
        await context.Contents
            .Include(c => c.Translations)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<Content?> GetByIdWithSectionsAsync(ContentId id, CancellationToken ct = default) =>
        await context.Contents
            .Include(c => c.Translations)
            .Include(c => c.Sections)
                .ThenInclude(s => s.Blocks)
                    .ThenInclude(b => b.Translations)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<Content?> GetBySlugAsync(
        string slug, Guid tenantId, CancellationToken ct = default) =>
        await context.Contents
            .Include(c => c.Translations)
            .FirstOrDefaultAsync(c =>
                c.Slug == Slug.Create(slug) &&
                c.TenantId == TenantId.From(tenantId),   // ← BUG FIX
                ct);

    public async Task<bool> SlugExistsAsync(
        string slug, Guid tenantId,
        ContentId? excludeId = null,
        CancellationToken ct = default)
    {
        var query = context.Contents
            .Where(c =>
                c.Slug == Slug.Create(slug) &&
                c.TenantId == TenantId.From(tenantId));   // ← BUG FIX

        if (excludeId is not null)
            query = query.Where(c => c.Id != excludeId);

        return await query.AnyAsync(ct);
    }

    // ── List (sayfalı + filtrelenmiş) ───────────────────────────────────────

    public async Task<Paginate<Content>> GetListAsync(
        string? search,
        ContentStatus? status,
        ContentType? contentType,
        int pageIndex,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = context.Contents
            .AsNoTracking()
            .Include(c => c.Translations)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c =>
                c.Translations.Any(t => t.Title.Contains(search)) ||
                c.Slug.Value.Contains(search));

        if (status.HasValue)
            query = query.Where(c => c.Status == status.Value);

        if (contentType.HasValue)                                // ← YENİ
            query = query.Where(c => c.ContentType == contentType.Value);

        return await query
            .OrderByDescending(c => c.CreatedAt)
            .ToPaginateAsync(pageIndex, pageSize, ct);
    }

    // ── Trash ───────────────────────────────────────────────────────────────
    public async Task<Content?> GetDeletedByIdAsync(ContentId id, CancellationToken ct = default) =>
        await context.Contents
            .IgnoreQueryFilters()
            .Include(c => c.Translations)
            .FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt != null, ct);

    public async Task<IReadOnlyList<Content>> GetTrashedAsync(
        TenantId tenantId, CancellationToken ct = default) =>
        await context.Contents
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(c => c.TenantId == tenantId && c.DeletedAt != null)
            .Include(c => c.Translations)
            .OrderByDescending(c => c.DeletedAt)
            .ToListAsync(ct);

    public async Task HardDeleteAsync(ContentId id, CancellationToken ct = default)
    {
        var content = await context.Contents
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (content is not null)
        {
            context.Contents.Remove(content);
            await context.SaveChangesAsync(ct);
        }
    }

    // ── Schedule watcher ────────────────────────────────────────────────────

    public async Task<IReadOnlyList<Content>> GetScheduledToPublishAsync(
        DateTime utcNow, CancellationToken ct = default) =>
        await context.Contents
            .IgnoreQueryFilters()
            .Where(c =>
                c.Status == ContentStatus.Scheduled &&
                c.Schedule != null &&
                c.Schedule.PublishAt <= utcNow &&
                c.DeletedAt == null)
            .Include(c => c.Translations)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Content>> GetScheduledToExpireAsync(
        DateTime utcNow, CancellationToken ct = default) =>
        await context.Contents
            .IgnoreQueryFilters()
            .Where(c =>
                c.Status == ContentStatus.Published &&
                c.Schedule != null &&
                c.Schedule.UnpublishAt != null &&
                c.Schedule.UnpublishAt <= utcNow &&
                c.DeletedAt == null)
            .Include(c => c.Translations)
            .ToListAsync(ct);

    // ── SEO ─────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<Content>> GetPublishedAsync(
        TenantId tenantId, CancellationToken ct = default) =>
        await context.Contents
            .AsNoTracking()
            .Where(c => c.Status == ContentStatus.Published)
            .Include(c => c.Translations)
            .ToListAsync(ct);

    // ── Translations ─────────────────────────────────────────────────────────

    public async Task<ContentTranslation?> GetTranslationAsync(
        ContentId id, IEnumerable<string> langCandidates, CancellationToken ct = default)
    {
        var translations = await context.Set<ContentTranslation>()
            .AsNoTracking()
            .Where(t => t.ContentId == id)
            .ToListAsync(ct);

        foreach (var lang in langCandidates)
        {
            var t = translations.FirstOrDefault(
                tr => tr.LanguageCode == lang.ToLowerInvariant());
            if (t is not null) return t;
        }
        return null;
    }

    // ── Write ────────────────────────────────────────────────────────────────

    public async Task<Content> AddAsync(Content content, CancellationToken ct = default)
    {
        await context.Contents.AddAsync(content, ct);
        await context.SaveChangesAsync(ct);
        return content;
    }

    public async Task<Content> UpdateAsync(Content content, CancellationToken ct = default)
    {
        context.Contents.Update(content);
        await context.SaveChangesAsync(ct);
        return content;
    }

    public Task<IReadOnlyList<Content>> GetScheduledInRangeAsync(TenantId tenantId, DateTime from, DateTime to, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}

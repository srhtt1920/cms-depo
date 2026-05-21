using CMS.Application.Features.Pages.GetPageBreadcrumb;
using CMS.Domain.Pages;
using CMS.Domain.Pages.Dtos;
using CMS.Domain.Tenants;
using Microsoft.EntityFrameworkCore;

namespace CMS.Infrastructure.Persistence.Repositories;

public sealed class PageRepository(CmsDbContext context) : IPageRepository
{
    // ── Single ───────────────────────────────────────────────────────────────

    public async Task<Page?> GetByIdAsync(PageId id, CancellationToken ct = default) =>
        await context.Pages
            .Include(p => p.Translations)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    // ── Slug benzersizlik ────────────────────────────────────────────────────

    public async Task<bool> SlugExistsAsync(
        string slug,
        string languageCode,
        Guid tenantId,
        PageId? excludePageId = null,
        CancellationToken ct = default)
    {
        // Navigation property (t.Page) yerine explicit join kullanıyoruz.
        // Include gerekmez; TenantId value-object karşılaştırması güvenli çalışır.
        var normalizedSlug = slug.ToLowerInvariant();
        var normalizedLang = languageCode.ToLowerInvariant();
        var tenantIdTyped = TenantId.From(tenantId);

        var query =
            from t in context.PageTranslations
            join p in context.Pages on t.PageId equals p.Id
            where t.Slug == normalizedSlug
               && t.LanguageCode == normalizedLang
               && p.TenantId == tenantIdTyped
               && p.DeletedAt == null
            select t.PageId;

        if (excludePageId is not null)
            query = query.Where(pid => pid != excludePageId);

        return await query.AnyAsync(ct);
    }

    // ── Tree — hafif (sadece yapısal alanlar) ────────────────────────────────

    public async Task<IReadOnlyList<PageFlatDto>> GetFlatAsync(
        TenantId tenantId, CancellationToken ct = default)
    {
        return await context.Pages
            .AsNoTracking()
            .Where(p => p.TenantId == tenantId)
            .OrderBy(p => p.Order)
            .Select(p => new PageFlatDto(
                p.Id.Value,
                p.ParentId == null ? (Guid?)null : p.ParentId.Value,
                p.PageType.ToString(),
                p.ContentType == null ? null : p.ContentType.ToString(),
                p.Order,
                p.IsActive,
                p.IsVisible,
                p.Icon))
            .ToListAsync(ct);
    }

    // ── Tree — detaylı (çevirilerle) ─────────────────────────────────────────

    public async Task<IReadOnlyList<PageFlatDetailDto>> GetFlatDetailAsync(
        TenantId tenantId, CancellationToken ct = default)
    {
        var pages = await context.Pages
            .AsNoTracking()
            .Include(p => p.Translations)
            .Where(p => p.TenantId == tenantId)
            .OrderBy(p => p.Order)
            .ToListAsync(ct);

        return pages.Select(p => new PageFlatDetailDto(
            p.Id.Value,
            p.ParentId?.Value,
            p.PageType.ToString(),
            p.ContentType?.ToString(),
            p.Order,
            p.IsActive,
            p.IsVisible,
            p.Icon,
            p.ExternalUrl,
            p.LinkedContentId?.Value,
            p.Translations.Select(t => new PageTranslationDto(
                t.LanguageCode, t.Title, t.LinkName, t.Slug,
                t.MetaTitle, t.MetaDescription, t.MetaKeywords
            )).ToList()
        )).ToList();
    }

    // ── Breadcrumb — SQL Server recursive CTE ────────────────────────────────

    public async Task<IReadOnlyList<PageBreadcrumbItemDto>> GetBreadcrumbAsync(
        PageId id, string languageCode, CancellationToken ct = default)
    {
        // SQL Server CTE syntax (WITH RECURSIVE yerine sadece WITH)
        var sql = @"
            WITH crumb AS (
                SELECT p.Id, p.ParentId, 0 AS Depth
                FROM Pages p
                WHERE p.Id = {0} AND p.DeletedAt IS NULL

                UNION ALL

                SELECT parent.Id, parent.ParentId, c.Depth + 1
                FROM Pages parent
                INNER JOIN crumb c ON parent.Id = c.ParentId
                WHERE parent.DeletedAt IS NULL
            )
            SELECT c.Id,
                   ISNULL(t.Title, '') AS Title,
                   ISNULL(t.Slug,  '') AS Slug,
                   c.Depth
            FROM crumb c
            LEFT JOIN PageTranslations t
                ON t.PageId       = c.Id
               AND t.LanguageCode = {1}
            ORDER BY c.Depth DESC";

        return await context.Database
            .SqlQueryRaw<BreadcrumbRawDto>(sql, id.Value, languageCode.ToLowerInvariant())
            .Select(r => new PageBreadcrumbItemDto(r.Id, r.Title, r.Slug, r.Depth))
            .ToListAsync(ct);
    }

    // ── Döngüsel referans kontrolü ───────────────────────────────────────────

    public async Task<IReadOnlyList<PageId>> GetDescendantIdsAsync(
        PageId id, CancellationToken ct = default)
    {
        // Tüm sayfaların id-parentId çiftlerini tek sorguda çek, bellek içinde traverse et.
        // Küçük-orta tree için (birkaç bin node) bu yeterince performanslı.
        var all = await context.Pages
            .AsNoTracking()
            .Select(p => new { Id = p.Id, ParentId = p.ParentId })
            .ToListAsync(ct);

        var result = new List<PageId>();
        var queue = new Queue<PageId>();
        queue.Enqueue(id);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var children = all.Where(p => p.ParentId == current);
            foreach (var child in children)
            {
                result.Add(child.Id);
                queue.Enqueue(child.Id);
            }
        }

        return result;
    }

    // ── Tree — alias (dashboard sayfa sayısı için) ────────────────────────────
    public async Task<IReadOnlyList<PageFlatDto>> GetTreeAsync(
        TenantId tenantId, bool includeInactive = false, CancellationToken ct = default)
    {
        var query = context.Pages
            .AsNoTracking()
            .Where(p => p.TenantId == tenantId);

        if (!includeInactive)
            query = query.Where(p => p.IsActive);

        return await query
            .OrderBy(p => p.Order)
            .Select(p => new PageFlatDto(
                p.Id.Value,
                p.ParentId == null ? (Guid?)null : p.ParentId.Value,
                p.PageType.ToString(),
                p.ContentType == null ? null : p.ContentType.ToString(),
                p.Order,
                p.IsActive,
                p.IsVisible,
                p.Icon))
            .ToListAsync(ct);
    }

    // ── Write ────────────────────────────────────────────────────────────────

    public async Task<Page> AddAsync(Page page, CancellationToken ct = default)
    {
        await context.Pages.AddAsync(page, ct);
        await context.SaveChangesAsync(ct);
        return page;
    }

    public async Task<Page> UpdateAsync(Page page, CancellationToken ct = default)
    {
        context.Pages.Update(page);
        await context.SaveChangesAsync(ct);
        return page;
    }
}
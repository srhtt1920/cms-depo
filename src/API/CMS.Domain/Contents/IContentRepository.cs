using CMS.Domain.Tenants;
using CMS.SharedKernel.Pagination;

namespace CMS.Domain.Contents;

public interface IContentRepository
{
    Task<Content?> GetByIdAsync(ContentId id, CancellationToken ct = default);
    Task<Content?> GetByIdWithSectionsAsync(ContentId id, CancellationToken ct = default);
    Task<Content?> GetBySlugAsync(string slug, Guid tenantId, CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slug, Guid tenantId, ContentId? excludeId = null, CancellationToken ct = default);

    /// <summary>Sayfalı + filtrelenmiş içerik listesi (admin paneli).</summary>
    Task<Paginate<Content>> GetListAsync(string? search, ContentStatus? status, ContentType? contentType, int pageIndex, int pageSize, CancellationToken ct = default);

    // Trash
    Task<Content?> GetDeletedByIdAsync(ContentId id, CancellationToken ct = default);
    Task<IReadOnlyList<Content>> GetTrashedAsync(TenantId tenantId, CancellationToken ct = default);
    Task HardDeleteAsync(ContentId id, CancellationToken ct = default);

    // Schedule watcher
    Task<IReadOnlyList<Content>> GetScheduledToPublishAsync(DateTime utcNow, CancellationToken ct = default);
    Task<IReadOnlyList<Content>> GetScheduledToExpireAsync(DateTime utcNow, CancellationToken ct = default);

    // SEO
    Task<IReadOnlyList<Content>> GetPublishedAsync(TenantId tenantId, CancellationToken ct = default);

    // Translations
    Task<ContentTranslation?> GetTranslationAsync(ContentId id, IEnumerable<string> langCandidates, CancellationToken ct = default);

    Task<Content> AddAsync(Content content, CancellationToken ct = default);
    Task<Content> UpdateAsync(Content content, CancellationToken ct = default);

    Task<IReadOnlyList<Content>> GetScheduledInRangeAsync(
    TenantId tenantId,
    DateTime from,
    DateTime to,
    CancellationToken ct = default);

}

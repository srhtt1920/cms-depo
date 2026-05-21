using CMS.Domain.Pages;

namespace CMS.Domain.Contents;

/// <summary>
/// İçerik ve sayfa kilit durumunu yöneten repository sözleşmesi.
/// Implementasyon Infrastructure katmanında yapılmalıdır.
/// </summary>
public interface IContentLockRepository
{
    Task<ContentLock?> GetByContentIdAsync(ContentId contentId, CancellationToken ct = default);
    Task<ContentLock?> GetByPageIdAsync(PageId pageId, CancellationToken ct = default);
    Task<IReadOnlyList<ContentLock>> GetByContentIdsAsync(IEnumerable<ContentId> ids, CancellationToken ct = default);
    Task UpsertAsync(ContentLock lockEntry, CancellationToken ct = default);
    Task DeleteByContentIdAsync(ContentId contentId, CancellationToken ct = default);
    Task DeleteByPageIdAsync(PageId pageId, CancellationToken ct = default);
    Task BulkUpsertAsync(IEnumerable<ContentLock> locks, CancellationToken ct = default);
}

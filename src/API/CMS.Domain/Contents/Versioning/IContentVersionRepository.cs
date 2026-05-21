namespace CMS.Domain.Contents.Versioning;

public interface IContentVersionRepository
{
    Task<List<ContentVersion>> GetByContentIdAsync(ContentId contentId, CancellationToken ct = default);
    Task<ContentVersion?> GetByNumberAsync(ContentId contentId, int versionNumber, CancellationToken ct = default);
    Task<int> GetLatestVersionNumberAsync(ContentId contentId, CancellationToken ct = default);
    Task AddAsync(ContentVersion version, CancellationToken ct = default);
}
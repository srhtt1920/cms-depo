using CMS.Domain.Contents;
using CMS.Domain.Contents.Versioning;
using Microsoft.EntityFrameworkCore;

namespace CMS.Infrastructure.Persistence.Repositories;

public sealed class ContentVersionRepository(CmsDbContext context)
 : IContentVersionRepository
{
    public async Task<List<ContentVersion>> GetByContentIdAsync(
        ContentId contentId, CancellationToken ct = default) =>
        await context.Set<ContentVersion>()
            .Where(v => v.ContentId == contentId)
            .OrderByDescending(v => v.VersionNumber)
            .ToListAsync(ct);

    public async Task<ContentVersion?> GetByNumberAsync(
        ContentId contentId, int versionNumber, CancellationToken ct = default) =>
        await context.Set<ContentVersion>()
            .FirstOrDefaultAsync(v =>
                v.ContentId == contentId &&
                v.VersionNumber == versionNumber, ct);

    public async Task<int> GetLatestVersionNumberAsync(
        ContentId contentId, CancellationToken ct = default)
    {
        var max = await context.Set<ContentVersion>()
            .Where(v => v.ContentId == contentId)
            .MaxAsync(v => (int?)v.VersionNumber, ct);
        return max ?? 0;
    }

    public async Task AddAsync(ContentVersion version, CancellationToken ct = default)
    {
        await context.Set<ContentVersion>().AddAsync(version, ct);
        await context.SaveChangesAsync(ct);
    }
}

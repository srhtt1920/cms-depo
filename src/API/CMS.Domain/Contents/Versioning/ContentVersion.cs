using CMS.Domain.Common;
using CMS.Domain.Identity;

namespace CMS.Domain.Contents.Versioning;

// <summary>
/// Her "Kaydet" işleminde snapshot alınır.
/// Snapshot: Content + tüm çeviriler JSON olarak saklanır.
/// </summary>
public sealed class ContentVersion : Entity<Guid>
{
    public ContentId ContentId { get; private set; } = default!;
    public int VersionNumber { get; private set; }
    public string Snapshot { get; private set; } = default!;
    public string? Label { get; private set; }
    public bool IsPublished { get; private set; }
    public UserId CreatedBy { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }

    private ContentVersion() { }

    public static ContentVersion Create(
        ContentId contentId,
        int versionNumber,
        string snapshot,
        UserId createdBy,
        bool isPublished = false,
        string? label = null)
    {
        return new ContentVersion
        {
            Id = Guid.NewGuid(),
            ContentId = contentId,
            VersionNumber = versionNumber,
            Snapshot = snapshot,
            Label = label,
            IsPublished = isPublished,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void SetLabel(string label) => Label = label;
}

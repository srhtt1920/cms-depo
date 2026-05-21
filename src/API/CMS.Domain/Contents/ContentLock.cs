using CMS.Domain.Identity;
using CMS.Domain.Pages;

namespace CMS.Domain.Contents;

public sealed class ContentLock
{
    public Guid Id { get; private set; }
    public ContentId? ContentId { get; private set; }
    public PageId? PageId { get; private set; }
    public string LockType { get; private set; } = "Soft"; // "Soft" | "Hard"
    public UserId LockedByUserId { get; private set; } = default!;
    public DateTime LockedAt { get; private set; }
    public string? LockReason { get; private set; }
    public bool RequiresSuperAdmin { get; private set; }

    private ContentLock() { }

    public static ContentLock ForContent(ContentId contentId, string lockType, UserId lockedBy, string? lockReason = null, bool requiresSuperAdmin = false) =>
        new() { Id = Guid.NewGuid(), ContentId = contentId, LockType = lockType, LockedByUserId = lockedBy, LockedAt = DateTime.UtcNow, LockReason = lockReason, RequiresSuperAdmin = requiresSuperAdmin };

    public static ContentLock ForPage(PageId pageId, string lockType, UserId lockedBy, string? lockReason = null, bool requiresSuperAdmin = false) =>
        new() { Id = Guid.NewGuid(), PageId = pageId, LockType = lockType, LockedByUserId = lockedBy, LockedAt = DateTime.UtcNow, LockReason = lockReason, RequiresSuperAdmin = requiresSuperAdmin };
}
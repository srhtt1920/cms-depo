namespace CMS.Application.Features.Contents.Lock;

public sealed record ContentLockDto(
   Guid EntityId,
   string EntityType,      // "Content" | "Page"
   bool IsLocked,
   string? LockType,        // "Soft" | "Hard" | null
   Guid? LockedByUserId,
   string? LockedByEmail,
   DateTime? LockedAt,
   string? LockReason = null,
   bool RequiresSuperAdmin = false);

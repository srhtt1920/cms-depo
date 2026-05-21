using CMS.Domain.Common;

namespace CMS.Domain.Identity;

public sealed class Permission : Entity<PermissionId>
{
    public string Key { get; private set; } = default!;       // "content.publish"
    public string GroupKey { get; private set; } = default!;  // "content"
    public string DisplayName { get; private set; } = default!;
    public PermissionId? ParentId { get; private set; }    // null = root node

    private Permission() { } // EF Core

    public static Permission Create(
        string key,
        string groupKey,
        string displayName,
        PermissionId? parentId = null)
    {
        return new Permission
        {
            Id = PermissionId.New(),
            Key = key,
            GroupKey = groupKey,
            DisplayName = displayName,
            ParentId = parentId
        };
    }
}

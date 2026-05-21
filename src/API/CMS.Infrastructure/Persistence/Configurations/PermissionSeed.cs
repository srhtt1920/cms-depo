namespace CMS.Infrastructure.Persistence.Configurations;

/// <summary>
/// Sistem permission'larını seed eder.
/// Bunlar sabit — kod değişmeden DB'de değişmez.
/// </summary>
internal static class PermissionSeed
{
    // Root group ID'leri
    private static readonly Guid ContentGroupId = new("10000000-0000-0000-0000-000000000001");
    private static readonly Guid IdentityGroupId = new("10000000-0000-0000-0000-000000000002");
    private static readonly Guid TenantGroupId = new("10000000-0000-0000-0000-000000000003");

    // Content permissions
    private static readonly Guid ContentReadId = new("20000000-0000-0000-0000-000000000001");
    private static readonly Guid ContentCreateId = new("20000000-0000-0000-0000-000000000002");
    private static readonly Guid ContentEditId = new("20000000-0000-0000-0000-000000000003");
    private static readonly Guid ContentPublishId = new("20000000-0000-0000-0000-000000000004");
    private static readonly Guid ContentArchiveId = new("20000000-0000-0000-0000-000000000005");

    // Identity permissions
    private static readonly Guid UsersManageId = new("30000000-0000-0000-0000-000000000001");
    private static readonly Guid RolesManageId = new("30000000-0000-0000-0000-000000000002");

    // Tenant permissions
    private static readonly Guid TenantsManageId = new("40000000-0000-0000-0000-000000000001");

    public static IEnumerable<object> GetSeedData() =>
    [
        // ── Content Group ────────────────────────────────────────
        new { Id = ContentGroupId,   Key = "content",         GroupKey = "content",  DisplayName = "Content Management",  ParentId = (Guid?)null },
        new { Id = ContentReadId,    Key = "content.read",    GroupKey = "content",  DisplayName = "Read content",        ParentId = (Guid?)ContentGroupId },
        new { Id = ContentCreateId,  Key = "content.create",  GroupKey = "content",  DisplayName = "Create content",      ParentId = (Guid?)ContentGroupId },
        new { Id = ContentEditId,    Key = "content.edit",    GroupKey = "content",  DisplayName = "Edit content",        ParentId = (Guid?)ContentGroupId },
        new { Id = ContentPublishId, Key = "content.publish", GroupKey = "content",  DisplayName = "Publish content",     ParentId = (Guid?)ContentGroupId },
        new { Id = ContentArchiveId, Key = "content.archive", GroupKey = "content",  DisplayName = "Archive content",     ParentId = (Guid?)ContentGroupId },

        // ── Identity Group ───────────────────────────────────────
        new { Id = IdentityGroupId,  Key = "users",           GroupKey = "identity", DisplayName = "User Management",    ParentId = (Guid?)null },
        new { Id = UsersManageId,    Key = "users.manage",    GroupKey = "identity", DisplayName = "Manage users",       ParentId = (Guid?)IdentityGroupId },
        new { Id = RolesManageId,    Key = "roles.manage",    GroupKey = "identity", DisplayName = "Manage roles",       ParentId = (Guid?)IdentityGroupId },

        // ── Tenant Group ─────────────────────────────────────────
        new { Id = TenantGroupId,    Key = "tenants",         GroupKey = "tenant",   DisplayName = "Tenant Management",  ParentId = (Guid?)null },
        new { Id = TenantsManageId,  Key = "tenants.manage",  GroupKey = "tenant",   DisplayName = "Manage tenants",     ParentId = (Guid?)TenantGroupId },
    ];
}

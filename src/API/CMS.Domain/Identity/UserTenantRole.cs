using CMS.Domain.Common;
using CMS.Domain.Tenants;

namespace CMS.Domain.Identity;

public sealed class UserTenantRole : Entity<Guid>
{
    public UserId UserId { get; private set; } = default!;
    public TenantId TenantId { get; private set; } = default!;
    public RoleId RoleId { get; private set; } = default!;
    public DateTime AssignedAt { get; private set; }

    private UserTenantRole() { } // EF Core

    public static UserTenantRole Create(UserId userId, TenantId tenantId, RoleId roleId) =>
        new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TenantId = tenantId,
            RoleId = roleId,
            AssignedAt = DateTime.UtcNow
        };
}

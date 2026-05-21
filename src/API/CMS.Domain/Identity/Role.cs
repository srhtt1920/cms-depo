using CMS.Domain.Common;
using CMS.Domain.Tenants;

namespace CMS.Domain.Identity;

public sealed class Role : AggregateRoot<RoleId>
{
    private readonly List<Permission> _permissions = [];

    public TenantId TenantId { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }

    public IReadOnlyList<Permission> Permissions => _permissions.AsReadOnly();

    private Role() { } // EF Core

    public static Role Create(TenantId tenantId, string name) =>
        new()
        {
            Id = RoleId.New(),
            TenantId = tenantId,
            Name = name,
            CreatedAt = DateTime.UtcNow
        };

    public void AddPermission(Permission permission)
    {
        if (_permissions.Any(p => p.Id == permission.Id)) return;
        _permissions.Add(permission);
    }

    public void RemovePermission(PermissionId permissionId) =>
        _permissions.RemoveAll(p => p.Id == permissionId);

    public void ClearPermissions() => _permissions.Clear();

    public void Rename(string name) => Name = name;

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;
}

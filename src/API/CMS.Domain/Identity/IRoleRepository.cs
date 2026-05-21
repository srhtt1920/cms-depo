using CMS.Domain.Tenants;

namespace CMS.Domain.Identity;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(RoleId id, CancellationToken ct = default);
    Task<IReadOnlyList<Role>> GetByTenantAsync(TenantId tenantId, CancellationToken ct = default);
    Task<Role?> GetWithPermissionsAsync(RoleId id, CancellationToken ct = default);
    Task<Role> AddAsync(Role role, CancellationToken ct = default);
    Task<Role> UpdateAsync(Role role, CancellationToken ct = default);
    /// <summary>Soft delete: IsActive = false yapar ve kullanıcılardan kaldırır.</summary>
    Task DeleteAsync(RoleId id, CancellationToken ct = default);
}

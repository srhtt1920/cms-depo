using CMS.Domain.Tenants;

namespace CMS.Domain.Identity;

public interface IPermissionRepository
{
    Task<IReadOnlyList<Permission>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetKeysByUserAndTenantAsync(UserId userId, TenantId tenantId, CancellationToken ct = default);
}

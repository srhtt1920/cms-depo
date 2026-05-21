using CMS.Domain.Tenants;

namespace CMS.Domain.Identity;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(UserId id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetWithRolesAsync(UserId id, CancellationToken ct = default);
    Task<User> AddAsync(User user, CancellationToken ct = default);
    Task<User> UpdateAsync(User user, CancellationToken ct = default);
    Task<IReadOnlyList<User>> GetByTenantAsync(TenantId tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<User>> GetSuperAdminsAsync(CancellationToken ct = default);
}

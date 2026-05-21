namespace CMS.Domain.Tenants;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(TenantId id, CancellationToken ct = default);
    Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<SupportedLanguage>> GetSupportedLanguagesAsync(TenantId id, CancellationToken ct = default);
    Task<Tenant> AddAsync(Tenant tenant, CancellationToken ct = default);
    Task<Tenant> UpdateAsync(Tenant tenant, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
}

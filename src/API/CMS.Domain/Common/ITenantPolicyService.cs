namespace CMS.Domain.Common;

/// <summary>
/// Tenant davranışlarını ve kısıtlamalarını merkezi olarak yönetir.
/// </summary>
public interface ITenantPolicyService
{
    /// <summary>System tenant'ın id'sini döner (DB'den veya cache'den).</summary>
    Task<Guid> GetSystemTenantIdAsync(CancellationToken ct = default);

    /// <summary>Verilen tenant'ın System olup olmadığını kontrol eder.</summary>
    Task<bool> IsSystemTenantAsync(Guid tenantId, CancellationToken ct = default);

    /// <summary>
    /// System tenant'a atanabilecek roller listesi.
    /// </summary>
    IReadOnlyList<string> AllowedSystemRoles { get; }

    /// <summary>
    /// Verilen rol adının System tenant'a atanmasına izin verip vermediğini döner.
    /// </summary>
    bool IsAllowedForSystemTenant(string roleName);
}

namespace CMS.Application.Common.Abstractions;

public interface IUserTenantAssignmentPolicy
{
    /// <summary>
    /// Verilen rol adının belirtilen tenant'a atanabilir olup olmadığını döner.
    /// System tenant için normal kullanıcı rolleri yasaktır.
    /// </summary>
    Task<bool> CanAssignRoleAsync(
        Guid tenantId, string roleName, CancellationToken ct = default);

    /// <summary>
    /// System tenant'a atanabilecek rollerin listesi.
    /// </summary>
    IReadOnlyList<string> GetAllowedRolesForSystemTenant();
}

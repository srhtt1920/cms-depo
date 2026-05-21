using CMS.Application.Common.Abstractions;
using CMS.Domain.Common;

namespace CMS.Infrastructure.Identity;

public sealed class UserTenantAssignmentPolicy(
    ITenantPolicyService tenantPolicy) : IUserTenantAssignmentPolicy
{
    public IReadOnlyList<string> GetAllowedRolesForSystemTenant() =>
        tenantPolicy.AllowedSystemRoles;

    public async Task<bool> CanAssignRoleAsync(
        Guid tenantId, string roleName, CancellationToken ct = default)
    {
        var isSystem = await tenantPolicy.IsSystemTenantAsync(tenantId, ct);
        if (!isSystem) return true; // Business tenant'ta her rol atanabilir

        return tenantPolicy.IsAllowedForSystemTenant(roleName);
    }
}

using CMS.Domain.Identity;
using CMS.Domain.Tenants;

namespace CMS.Application.Common.Abstractions;

public interface IJwtService
{
    string GenerateToken(User user, Guid? activeTenantId = null);
    string GenerateToken(User user, Guid activeTenantId, string tenantName, TenantType tenantType);
    DateTime GetExpiry();
    Task<(string AccessToken, string RefreshToken)> GenerateTokenPairAsync(
    User user, CancellationToken ct = default);
}
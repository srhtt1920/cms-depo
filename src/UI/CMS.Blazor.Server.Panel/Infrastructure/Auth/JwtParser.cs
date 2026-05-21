using System.IdentityModel.Tokens.Jwt;

namespace CMS.Blazor.Server.Panel.Infrastructure.Auth;

public sealed record JwtClaims(
    string Email,
    int PermissionVersion,
    Guid? TenantId,
    DateTime ValidTo);

public static class JwtParser
{
    private static readonly JwtSecurityTokenHandler Handler = new();

    public static JwtClaims? Parse(string token)
    {
        if (string.IsNullOrEmpty(token)) return null;
        try
        {
            var jwt = Handler.ReadJwtToken(token);
            if (jwt.ValidTo < DateTime.UtcNow) return null;

            var email = jwt.Claims
                .FirstOrDefault(c => c.Type is "email"
                    or "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")
                ?.Value ?? string.Empty;

            var permV = jwt.Claims.FirstOrDefault(c => c.Type == "permV")?.Value;
            var tid = jwt.Claims.FirstOrDefault(c => c.Type == "tenantId")?.Value;

            return new JwtClaims(
                email,
                int.TryParse(permV, out var v) ? v : 0,
                Guid.TryParse(tid, out var g) ? g : null,
                jwt.ValidTo);
        }
        catch { return null; }
    }
}

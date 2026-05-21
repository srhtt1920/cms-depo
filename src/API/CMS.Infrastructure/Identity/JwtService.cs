using CMS.Application.Common.Abstractions;
using CMS.Domain.Common;
using CMS.Domain.Identity;
using CMS.Domain.Tenants;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CMS.Infrastructure.Identity;

public sealed class JwtService(
    IConfiguration configuration,
    IRefreshTokenRepository refreshTokenRepo) : IJwtService
{
    private readonly string _secretKey = configuration["Jwt:SecretKey"]!;
    private readonly string _issuer = configuration["Jwt:Issuer"]!;
    private readonly string _audience = configuration["Jwt:Audience"]!;
    private readonly int _expiryMins = int.Parse(configuration["Jwt:ExpiryMinutes"] ?? "60");


    public string GenerateToken(User user, Guid? activeTenantId = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.Value.ToString()),
            new(ClaimTypes.Email,          user.Email),
            new("email",                   user.Email),
            new("permV",                   user.PermissionVersion.ToString()),
        };

        // Tüm tenant'ları listeye ekle
        foreach (var tenantId in user.GetTenantIds())
            claims.Add(new("tenantList", tenantId.Value.ToString()));

        // Aktif tenant seçildiyse tenantId + tenantName claim'lerini ekle
        // TenantResolutionBehavior bu claim'leri okur — DB çağrısına gerek kalmaz
        if (activeTenantId.HasValue)
        {
            claims.Add(new("tenantId", activeTenantId.Value.ToString()));
            // tenantName SelectTenant handler'dan geçirilir (aşağıda overload)
        }

        return BuildToken(claims);
    }

    /// <summary>
    /// TenantName'i de claim'e ekleyen overload — SelectTenant handler tarafından çağrılır.
    /// </summary>
    public string GenerateToken(User user, Guid activeTenantId, string tenantName,
        TenantType tenantType = TenantType.Business)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.Value.ToString()),
            new(ClaimTypes.Email,          user.Email),
            new("email",                   user.Email),
            new("permV",                   user.PermissionVersion.ToString()),
            new("tenantId",                activeTenantId.ToString()),
            new("tenantName",              tenantName),
            new("tenantType",              ((int)tenantType).ToString()),
        };

        foreach (var tid in user.GetTenantIds())
            claims.Add(new("tenantList", tid.Value.ToString()));

        return BuildToken(claims);
    }

    public DateTime GetExpiry() =>
        DateTime.UtcNow.AddMinutes(_expiryMins);

    /// <inheritdoc />
    public async Task<(string AccessToken, string RefreshToken)> GenerateTokenPairAsync(
        User user, CancellationToken ct = default)
    {
        var accessToken = GenerateToken(user);

        var refreshToken = Domain.Identity.RefreshToken.Create(user.Id);
        await refreshTokenRepo.AddAsync(refreshToken, ct);

        return (accessToken, refreshToken.Token);
    }

    private string BuildToken(List<Claim> claims)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: GetExpiry(),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
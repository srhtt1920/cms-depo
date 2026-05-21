using CMS.Application.Common.Abstractions;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace CMS.Infrastructure.Identity;

public sealed class CurrentUser(IHttpContextAccessor http) : ICurrentUser
{
    private ClaimsPrincipal? Principal => http.HttpContext?.User;

    public Guid UserId =>
        Guid.TryParse(Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id : Guid.Empty;

    public string Email =>
        Principal?.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

    public int PermissionVersion =>
        int.TryParse(Principal?.FindFirstValue("permV"), out var v) ? v : 0;

    public bool IsAuthenticated =>
        Principal?.Identity?.IsAuthenticated ?? false;

    public IReadOnlyList<Guid> TenantIds =>
        Principal?.FindAll("tenantId")
            .Select(c => Guid.TryParse(c.Value, out var g) ? g : Guid.Empty)
            .Where(g => g != Guid.Empty)
            .ToList() ?? [];

    // Select-tenant sonrasý token'da "tenantId" claim'i bulunur
    public Guid CurrentTenantId =>
        Guid.TryParse(Principal?.FindFirstValue("tenantId"), out var g) ? g : Guid.Empty;

}

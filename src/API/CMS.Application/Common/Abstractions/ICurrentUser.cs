namespace CMS.Application.Common.Abstractions;
public interface ICurrentUser
{
    Guid UserId { get; }
    string Email { get; }
    int PermissionVersion { get; }
    bool IsAuthenticated { get; }
    Guid CurrentTenantId { get; }          // JWT'deki "tenantId" claim'i
    IReadOnlyList<Guid> TenantIds { get; } // JWT'deki "tenantList" claim'leri
}


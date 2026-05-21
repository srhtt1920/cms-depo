namespace CMS.Application.Common.Abstractions;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(Guid userId, Guid tenantId, string permissionKey, CancellationToken ct = default);
    Task InvalidateAsync(Guid userId, Guid tenantId, CancellationToken ct = default);
}

using CMS.Application.Common.Abstractions;
using CMS.Domain.Common;
using Microsoft.AspNetCore.Http;

namespace CMS.Infrastructure.Identity;

public sealed class AuditLogger(
    IAuditLogRepository auditLogRepository,
    ITenantContext tenantContext,
    ICurrentUser currentUser,
    IHttpContextAccessor http)
    : IAuditLogger
{
    public async Task LogAsync(
        string action,
        string entityType,
        string? entityId = null,
        string? details = null,
        CancellationToken ct = default)
    {
        var ip = http.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "";
        var log = AuditLog.Create(
            tenantContext.TenantId,
            currentUser.IsAuthenticated ? currentUser.UserId : null,
            currentUser.IsAuthenticated ? currentUser.Email : "anonymous",
            action, entityType, entityId, details, ip);

        await auditLogRepository.AddAsync(log, ct);
    }
}

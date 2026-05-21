namespace CMS.Domain.Common;

public interface IAuditLogRepository
{
    Task<(List<AuditLog> Items, int TotalCount)> GetPagedAsync(
        Guid? tenantId,
        Guid? userId,
        string? entityType,
        string? action,
        int pageIndex,
        int pageSize,
        CancellationToken ct = default);

    Task AddAsync(AuditLog log, CancellationToken ct = default);
}

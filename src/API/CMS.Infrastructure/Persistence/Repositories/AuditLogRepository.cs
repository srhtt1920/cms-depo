using CMS.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace CMS.Infrastructure.Persistence.Repositories;

public sealed class AuditLogRepository(CmsDbContext context) : IAuditLogRepository
{
    public async Task<(List<AuditLog> Items, int TotalCount)> GetPagedAsync(
        Guid? tenantId,
        Guid? userId,
        string? entityType,
        string? action,
        int pageIndex,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = context.Set<AuditLog>()
            .AsNoTracking()
            .Where(l => l.TenantId == tenantId);

        if (userId.HasValue) query = query.Where(l => l.UserId == userId.Value);
        if (!string.IsNullOrEmpty(entityType)) query = query.Where(l => l.EntityType == entityType);
        if (!string.IsNullOrEmpty(action)) query = query.Where(l => l.Action.Contains(action));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task AddAsync(AuditLog log, CancellationToken ct = default)
    {
        await context.Set<AuditLog>().AddAsync(log, ct);
        await context.SaveChangesAsync(ct);
    }
}

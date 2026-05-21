namespace CMS.Application.Common.Abstractions;

public interface IAuditLogger
{
    Task LogAsync(
        string action,
        string entityType,
        string? entityId = null,
        string? details = null,
        CancellationToken ct = default);
}

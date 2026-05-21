namespace CMS.Domain.Common;

/// <summary>
/// Tüm önemli işlemlerin kaydı. Tenant bazlı ve kullanıcı bazlı sorgulanabilir.
/// Read-only — güncelleme ve silme yok.
/// </summary>
public sealed class AuditLog
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid? UserId { get; private set; }
    public string UserEmail { get; private set; } = default!;
    public string Action { get; private set; } = default!; // "content.publish"
    public string EntityType { get; private set; } = default!; // "Content"
    public string? EntityId { get; private set; }
    public string? Details { get; private set; }             // JSON extra info
    public string IpAddress { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }

    private AuditLog() { }

    public static AuditLog Create(
        Guid tenantId,
        Guid? userId,
        string userEmail,
        string action,
        string entityType,
        string? entityId = null,
        string? details = null,
        string ipAddress = "")
    {
        return new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userId,
            UserEmail = userEmail,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details,
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow
        };
    }
}

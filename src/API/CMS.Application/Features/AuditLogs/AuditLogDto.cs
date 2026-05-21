namespace CMS.Application.Features.AuditLogs;

public sealed record AuditLogDto(
 Guid Id,
 string UserEmail,
 string Action,
 string EntityType,
 string? EntityId,
 string? Details,
 string IpAddress,
 DateTime CreatedAt);

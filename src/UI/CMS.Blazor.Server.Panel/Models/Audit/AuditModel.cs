namespace CMS.Blazor.Server.Panel.Models.Audit;

public sealed record AuditLogPagedDto(
    int Index, int Size, int Count, int Pages,
    bool HasPrevious, bool HasNext,
    List<AuditLogDto> Items);

public sealed record AuditLogDto(
    Guid Id, string UserEmail, string Action,
    string EntityType, string? EntityId,
    string? Details, string IpAddress, DateTime CreatedAt);

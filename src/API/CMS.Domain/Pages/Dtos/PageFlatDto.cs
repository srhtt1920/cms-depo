namespace CMS.Domain.Pages.Dtos;

// ── Hafif flat DTO (tree/light endpoint) ─────────────────────────────────────
public sealed record PageFlatDto(
    Guid Id,
    Guid? ParentId,
    string PageType,
    string? ContentType,
    int Order,
    bool IsActive,
    bool IsVisible,
    string? Icon);

namespace CMS.Domain.Pages.Dtos;

// ── Detaylı flat DTO (tree/detail endpoint) ──────────────────────────────────
public sealed record PageFlatDetailDto(
    Guid Id,
    Guid? ParentId,
    string PageType,
    string? ContentType,
    int Order,
    bool IsActive,
    bool IsVisible,
    string? Icon,
    string? ExternalUrl,
    Guid? LinkedContentId,
    IReadOnlyList<PageTranslationDto> Translations);

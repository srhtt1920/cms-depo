namespace CMS.Application.Features.Pages.GetPageById;

public sealed record PageDetailDto(
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
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<PageTranslationDetailDto> Translations);
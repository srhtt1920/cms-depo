namespace CMS.Application.Features.Pages.GetPageTreeDetail;

public sealed record PageTreeDetailNodeDto(
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
    IReadOnlyList<PageTranslationResponseDto> Translations,
    List<PageTreeDetailNodeDto> Children);

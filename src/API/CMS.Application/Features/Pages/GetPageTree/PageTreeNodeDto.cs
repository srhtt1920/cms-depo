namespace CMS.Application.Features.Pages.GetPageTree;

public sealed record PageTreeNodeDto(
    Guid Id,
    Guid? ParentId,
    string PageType,
    string? ContentType,
    int Order,
    bool IsActive,
    bool IsVisible,
    string? Icon,
    List<PageTreeNodeDto> Children);


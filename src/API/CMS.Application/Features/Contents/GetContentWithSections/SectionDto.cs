namespace CMS.Application.Features.Contents.GetContentWithSections;

public sealed record SectionDto(
    Guid Id,
    string Name,
    int Order,
    bool IsVisible,
    bool IsEnabled,
    string? CssClass,
    Guid? ParentSectionId,
    AnimationDto? Animation,
    List<BlockDto> Blocks);

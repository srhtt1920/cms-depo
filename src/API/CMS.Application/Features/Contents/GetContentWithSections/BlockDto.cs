namespace CMS.Application.Features.Contents.GetContentWithSections;

public sealed record BlockDto(
    Guid Id,
    string BlockType,
    int Order,
    bool IsVisible,
    string Settings,
    List<BlockTransDto> Translations);

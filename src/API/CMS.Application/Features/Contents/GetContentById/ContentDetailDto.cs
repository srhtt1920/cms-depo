namespace CMS.Application.Features.Contents.GetContentById;

public sealed record ContentDetailDto(
    Guid Id,
    string Slug,
    string Status,
    string ContentType,
    Guid? AuthorId,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<ContentTranslationDto> Translations,
    ContentScheduleDto? Schedule);

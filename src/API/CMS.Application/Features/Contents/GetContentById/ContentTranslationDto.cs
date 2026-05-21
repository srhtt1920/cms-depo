namespace CMS.Application.Features.Contents.GetContentById;

public sealed record ContentTranslationDto(
    string LanguageCode,
    string Title,
    string Body,
    string? MetaTitle,
    string? MetaDescription,
    bool IsPublished);

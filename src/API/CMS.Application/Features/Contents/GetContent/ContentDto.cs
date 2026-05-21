namespace CMS.Application.Features.Contents.GetContent;

public sealed record ContentDto(
    Guid Id,
    string Slug,
    string LanguageCode,
    string Title,
    string Body,
    string? MetaTitle,
    string? MetaDescription,
    string Status,
    DateTime? PublishedAt);

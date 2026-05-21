namespace CMS.Application.Features.Contents.UpdateContent;

public sealed record UpdateContentRequest(
    string LanguageCode,
    string Title,
    string Body,
    string? MetaTitle,
    string? MetaDescription);
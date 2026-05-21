namespace CMS.Application.Features.Contents.AddTranslation;

public sealed record AddTranslationRequest(
    string LanguageCode,
    string Title,
    string Body,
    string? MetaTitle,
    string? MetaDescription);

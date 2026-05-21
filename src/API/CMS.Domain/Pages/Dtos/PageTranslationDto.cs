namespace CMS.Domain.Pages.Dtos;

public sealed record PageTranslationDto(
    string LanguageCode,
    string Title,
    string LinkName,
    string Slug,
    string? MetaTitle,
    string? MetaDescription,
    string? MetaKeywords);

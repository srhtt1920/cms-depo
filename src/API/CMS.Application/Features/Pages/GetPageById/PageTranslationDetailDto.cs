namespace CMS.Application.Features.Pages.GetPageById;

public sealed record PageTranslationDetailDto(
    string LanguageCode,
    string Title,
    string LinkName,
    string Slug,
    string? MetaTitle,
    string? MetaDescription,
    string? MetaKeywords);

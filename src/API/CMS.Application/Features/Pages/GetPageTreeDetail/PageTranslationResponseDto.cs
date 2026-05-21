namespace CMS.Application.Features.Pages.GetPageTreeDetail;

public sealed record PageTranslationResponseDto(
  string LanguageCode,
  string Title,
  string LinkName,
  string Slug,
  string? MetaTitle,
  string? MetaDescription,
  string? MetaKeywords);

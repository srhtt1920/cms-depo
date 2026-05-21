namespace CMS.Application.Features.Pages.UpsertPageTranslation;

public sealed record UpsertPageTranslationRequest(
  string LanguageCode,
  string Title,
  string LinkName,
  string Slug,
  string? MetaTitle,
  string? MetaDescription,
  string? MetaKeywords);

namespace CMS.Application.Features.Tenants.UpdateLanguages;

public sealed record UpdateLanguagesRequest(
  List<string> LanguageCodes,
  string DefaultLanguageCode);

namespace CMS.Application.Features.Approvals;

public sealed record ApprovalTranslationDto(
 string LanguageCode,
 string Title,
 string Body);

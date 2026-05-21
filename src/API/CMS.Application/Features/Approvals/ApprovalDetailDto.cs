using CMS.Application.Features.Approvals;

public sealed record ApprovalDetailDto(
    Guid ContentId,
    string Slug,
    string ContentType,
    List<ApprovalTranslationDto> Translations,
    string Status,
    Guid SubmittedByUserId,
    string SubmittedByEmail,
    DateTime SubmittedAt,
    string? Note,
    Guid? ReviewedByUserId,
    string? ReviewedByEmail,
    DateTime? ReviewedAt,
    string? ReviewComment);
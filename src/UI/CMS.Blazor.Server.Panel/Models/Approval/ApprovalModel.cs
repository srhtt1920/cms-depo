namespace CMS.Blazor.Server.Panel.Models.Approval;
public sealed record ApprovalItemDto(
    Guid ContentId, string Slug, string Title, string Status,
    Guid SubmittedByUserId, string SubmittedByEmail,
    DateTime SubmittedAt, string? Note);

public sealed record ApprovalDetailDto(
    Guid ContentId, string Slug, string ContentType,
    List<ApprovalTranslationDto> Translations,
    string Status,
    Guid SubmittedByUserId, string SubmittedByEmail,
    DateTime SubmittedAt, string? Note,
    Guid? ReviewedByUserId, string? ReviewedByEmail,
    DateTime? ReviewedAt, string? ReviewComment);

public sealed record ApprovalTranslationDto(string LanguageCode, string Title, string Body);

public sealed record ApprovalPagedDto(
    int Index, int Size, int Count, int Pages,
    bool HasPrevious, bool HasNext,
    List<ApprovalItemDto> Items);

public sealed record ApproveRequest(string? Comment = null);
public sealed record RejectRequest(string Reason);
namespace CMS.Application.Features.Approvals;

public sealed record ApprovalItemDto(
   Guid ContentId,
   string Slug,
   string Title,
   string Status,
   Guid SubmittedByUserId,
   string SubmittedByEmail,
   DateTime SubmittedAt,
   string? Note);

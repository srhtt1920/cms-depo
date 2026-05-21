using CMS.Domain.Identity;

namespace CMS.Domain.Contents;

public sealed class ApprovalInfo
{
    public UserId SubmittedByUserId { get; private set; } = default!;
    public DateTime SubmittedAt { get; private set; }
    public string? Note { get; private set; }

    public UserId? ReviewedByUserId { get; private set; }
    public DateTime? ReviewedAt { get; private set; }
    public string? ReviewComment { get; private set; }

    /// <summary>Onay durumu: Pending | Approved | Rejected</summary>
    public string ApprovalStatus { get; private set; } = "Pending";

    private ApprovalInfo() { } // EF Core

    /// <summary>Onaya gönderildiğinde oluştur.</summary>
    public static ApprovalInfo Pending(UserId submittedBy, string? note = null) =>
        new()
        {
            SubmittedByUserId = submittedBy,
            SubmittedAt = DateTime.UtcNow,
            Note = note,
            ApprovalStatus = "Pending"
        };

    /// <summary>Onaylandığında yeni bir örnek döndür (immutable style).</summary>
    public ApprovalInfo Accept(UserId reviewedBy, string? comment = null) =>
        new()
        {
            SubmittedByUserId = SubmittedByUserId,
            SubmittedAt = SubmittedAt,
            Note = Note,
            ReviewedByUserId = reviewedBy,
            ReviewedAt = DateTime.UtcNow,
            ReviewComment = comment,
            ApprovalStatus = "Approved"
        };

    /// <summary>Reddedildiğinde yeni bir örnek döndür (immutable style).</summary>
    public ApprovalInfo Reject(UserId reviewedBy, string reason) =>
        new()
        {
            SubmittedByUserId = SubmittedByUserId,
            SubmittedAt = SubmittedAt,
            Note = Note,
            ReviewedByUserId = reviewedBy,
            ReviewedAt = DateTime.UtcNow,
            ReviewComment = reason,
            ApprovalStatus = "Rejected"
        };
}

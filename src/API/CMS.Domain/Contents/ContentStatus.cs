namespace CMS.Domain.Contents;

public enum ContentStatus
{
    Draft     = 0,
    Scheduled = 1,   // PublishAt gelecekte
    Published = 2,
    Expired   = 3,   // UnpublishAt geçti, arşivlenmedi
    Archived  = 4,
    PendingApproval = 5,   // ← YENİ
    Rejected = 6,
}

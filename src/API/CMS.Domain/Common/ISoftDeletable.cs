namespace CMS.Domain.Common;

public interface ISoftDeletable
{
    DateTime? DeletedAt { get; }
    bool IsDeleted => DeletedAt.HasValue;
}

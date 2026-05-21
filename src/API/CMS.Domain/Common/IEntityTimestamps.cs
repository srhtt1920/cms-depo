namespace CMS.Domain.Common;

public interface IEntityTimestamps
{
    DateTime CreatedAt { get; }
    DateTime? UpdatedAt { get; }
    DateTime? DeletedAt { get; }
}

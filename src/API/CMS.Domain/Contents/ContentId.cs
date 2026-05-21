using CMS.Domain.Common;

namespace CMS.Domain.Contents;

public sealed record ContentId(Guid Value) : ValueObject
{
    public static ContentId New() => new(Guid.NewGuid());
    public static ContentId From(Guid value) => new(value);
    public static implicit operator Guid(ContentId id) => id.Value;
    public static explicit operator ContentId(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

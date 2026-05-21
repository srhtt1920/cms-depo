using CMS.Domain.Common;

namespace CMS.Domain.Identity;

public sealed record UserId(Guid Value) : ValueObject
{
    public static UserId New() => new(Guid.NewGuid());
    public static UserId From(Guid value) => new(value);
    public static implicit operator Guid(UserId id) => id.Value;
    public static explicit operator UserId(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

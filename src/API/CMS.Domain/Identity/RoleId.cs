using CMS.Domain.Common;

namespace CMS.Domain.Identity;

public sealed record RoleId(Guid Value) : ValueObject
{
    public static RoleId New() => new(Guid.NewGuid());
    public static RoleId From(Guid value) => new(value);
    public static implicit operator Guid(RoleId id) => id.Value;
    public static explicit operator RoleId(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

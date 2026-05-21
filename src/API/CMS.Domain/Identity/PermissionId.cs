using CMS.Domain.Common;

namespace CMS.Domain.Identity;

public sealed record PermissionId(Guid Value) : ValueObject
{
    public static PermissionId New() => new(Guid.NewGuid());
    public static PermissionId From(Guid value) => new(value);
    public static implicit operator Guid(PermissionId id) => id.Value;
    public override string ToString() => Value.ToString();
}

using CMS.Domain.Common;

namespace CMS.Domain.Tenants;

public sealed record TenantId(Guid Value) : ValueObject
{
    public static TenantId New() => new(Guid.NewGuid());
    public static TenantId From(Guid value) => new(value);

    public static implicit operator Guid(TenantId id) => id.Value;
    public static explicit operator TenantId(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}

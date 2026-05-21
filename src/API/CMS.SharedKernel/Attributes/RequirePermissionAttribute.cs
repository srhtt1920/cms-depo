namespace CMS.SharedKernel.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class RequirePermissionAttribute(string permissionKey) : Attribute
{
    public string PermissionKey { get; } = permissionKey;
}
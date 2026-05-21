namespace CMS.Domain.Identity;

public sealed class PermissionNode
{
    public string Key { get; init; } = default!;
    public string DisplayName { get; init; } = default!;
    public bool Granted { get; init; }
    public IReadOnlyList<PermissionNode> Children { get; init; } = [];
}

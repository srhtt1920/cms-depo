namespace CMS.Domain.Identity;

public static class PermissionTree
{
    public static IReadOnlyList<PermissionNode> Build(
        IEnumerable<Permission> allPermissions,
        IEnumerable<string> grantedKeys)
    {
        var granted = new HashSet<string>(grantedKeys, StringComparer.OrdinalIgnoreCase);
        var perms   = allPermissions.ToList();

        var lookup = perms
            .Where(p => p.ParentId != null)
            .GroupBy(p => p.ParentId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        PermissionNode BuildNode(Permission p)
        {
            var children  = lookup.TryGetValue(p.Id, out var list)
                ? list.Select(BuildNode).ToList()
                : new List<PermissionNode>();
            var isGranted = granted.Contains(p.Key);
            return new PermissionNode
            {
                Key         = p.Key,
                DisplayName = p.DisplayName,
                Children    = children,
                Granted     = isGranted || children.Any(c => c.Granted)
            };
        }

        return perms.Where(p => p.ParentId == null)
                    .Select(BuildNode)
                    .ToList();
    }
}

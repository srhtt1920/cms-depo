using CMS.Blazor.Server.Panel.Infrastructure.ApiClients.Auth;
using CMS.Blazor.Server.Panel.Infrastructure.Auth;
using CMS.Blazor.Server.Panel.Models.Shared;
using CMS.Blazor.Server.Panel.Services.State;

namespace CMS.Blazor.Server.Panel.Services;

public sealed class PermissionSyncService(AuthApiClient api, ApplicationState appState)
{
    public async Task SyncIfNeededAsync(CancellationToken ct = default)
    {
        var session = appState.Session;
        if (!session.IsAuthenticated || !session.HasTenant) return;

        var claims = JwtParser.Parse(session.Token);
        if (claims is null) return;

        // JWT'deki permV ile bellekteki version karþýlaþtýr
        if (claims.PermissionVersion == session.PermissionVersion) return;

        var result = await api.GetPermissionsAsync(ct);
        if (!result.IsSuccess || result.Data is null) return;

        var flat = FlattenGranted(result.Data.Tree);
        await appState.UpdatePermissionsAsync(flat, result.Data.Version);
    }

    /// <summary>
    /// Permission tree'yi düzleþtirir — Granted=true olan key'leri toplar.
    /// Children null olabilir (API leaf node'larda null döner).
    /// </summary>
    public static HashSet<string> FlattenGranted(
        IReadOnlyList<PermissionNodeDto>? nodes)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        Walk(nodes, set);
        return set;
    }

    private static void Walk(IReadOnlyList<PermissionNodeDto>? list, HashSet<string> set)
    {
        if (list is null) return;
        foreach (var n in list)
        {
            if (n.Granted) set.Add(n.Key);
            Walk(n.Children, set);   // Children nullable — null safe
        }
    }
}

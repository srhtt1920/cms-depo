using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CMS.Infrastructure.SignalR.Hubs;

/// <summary>
/// Client'ın implemente etmesi gereken metotlar.
/// </summary>
public interface IPermissionClient
{
    /// <summary>
    /// Panel client'ı bu eventi dinler.
    /// Gelince /api/me/permissions endpoint'ini yeniden çeker.
    /// </summary>
    Task OnPermissionVersionChanged(Guid userId, Guid tenantId, int newVersion);
}

/// <summary>
/// Permission invalidation hub'ı.
/// Panel frontend login sonrası bu hub'a bağlanır ve "user:{userId}" grubuna katılır.
/// </summary>
[Authorize]
public sealed class PermissionHub : Hub<IPermissionClient>
{
    /// <summary>
    /// Bağlantı kurulunca kullanıcı kendi grubuna katılır.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Bağlantı kopunca gruptan çıkar.
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user:{userId}");

        await base.OnDisconnectedAsync(exception);
    }
}

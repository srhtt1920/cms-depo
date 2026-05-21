using CMS.Blazor.Server.Panel.Infrastructure.Http.Base;
using CMS.Blazor.Server.Panel.Models.Shared;
using CMS.Blazor.Server.Panel.Models.User;
using CMS.Blazor.Server.Panel.Services.State;

namespace CMS.Blazor.Server.Panel.Infrastructure.ApiClients.Identity;

/// <summary>
/// Kullanıcı davet sistemi: oluştur, listele, iptal et, tekrar gönder.
/// Endpointler: <see cref="ApiEndpoints.Users"/> (Invite bölümü)
/// </summary>
public sealed class InviteApiClient(IHttpClientFactory factory, ApplicationState appState)
    : ApiClientBase(factory, appState)
{
    // ── Tenant admininin kullandığı: davet oluştur ─────────────────────────

    /// <summary>Yeni kullanıcı daveti oluşturur ve e-posta gönderir.</summary>
    public Task<ApiResult<InviteDto>> SendInviteAsync(
        InviteUserRequest req, CancellationToken ct = default) =>
        PostAsync<InviteUserRequest, InviteDto>(
            ApiEndpoints.Users.Invite, req, ct: ct);

    /// <summary>Tenant'a ait bekleyen/tüm davetleri listeler.</summary>
    public Task<ApiResult<List<InviteDto>>> GetInvitesAsync(
        InviteStatus? status = null,
        CancellationToken ct = default)
    {
        var q = status.HasValue ? $"?status={status.Value}" : string.Empty;
        return GetAsync<List<InviteDto>>($"{ApiEndpoints.Users.Invites}{q}", ct);
    }

    /// <summary>Belirli bir daveti ID ile getirir.</summary>
    public Task<ApiResult<InviteDto>> GetInviteByIdAsync(
        Guid inviteId, CancellationToken ct = default) =>
        GetAsync<InviteDto>(ApiEndpoints.Users.InviteById(inviteId), ct);

    /// <summary>Token ile daveti getirir (public, auth gerektirmez).</summary>
    public Task<ApiResult<InviteDto>> GetInviteByTokenAsync(
        string token, CancellationToken ct = default) =>
        GetAsync<InviteDto>($"api/users/invites/token/{token}", ct);

    /// <summary>Daveti iptal eder (PendingInvite → Cancelled).</summary>
    public Task<ApiResult<bool>> CancelInviteAsync(
        Guid inviteId, CancellationToken ct = default) =>
        DeleteAsync<bool>(ApiEndpoints.Users.InviteCancel(inviteId), ct);

    /// <summary>Daveti tekrar gönderir, expiry yenilenir.</summary>
    public Task<ApiResult<bool>> ResendInviteAsync(
        Guid inviteId, CancellationToken ct = default) =>
        PostAsync<ResendInviteRequest, bool>(
            ApiEndpoints.Users.InviteResend(inviteId),
            new ResendInviteRequest(inviteId), ct: ct);

    // ── Kullanıcının kullandığı: daveti kabul/ret ──────────────────────────

    /// <summary>
    /// Daveti kabul eder. Token e-posta linkinden gelir.
    /// Kullanıcı şifresini burada belirler.
    /// </summary>
    public Task<ApiResult<bool>> AcceptInviteAsync(
        AcceptInviteRequest req, CancellationToken ct = default) =>
        PostAsync<AcceptInviteRequest, bool>(
            ApiEndpoints.Users.InviteAccept(req.Token), req, ct: ct);

    /// <summary>Daveti reddeder.</summary>
    public Task<ApiResult<bool>> RejectInviteAsync(
        string token, CancellationToken ct = default) =>
        PostAsync<object, bool>(
            ApiEndpoints.Users.InviteReject(token), new { }, ct: ct);

    // ── Membership ─────────────────────────────────────────────────────────

    /// <summary>Kullanıcının üye olduğu tüm tenant'ları döner.</summary>
    public Task<ApiResult<List<UserTenantMembershipDto>>> GetUserTenantsAsync(
        Guid userId, CancellationToken ct = default) =>
        GetAsync<List<UserTenantMembershipDto>>(
            ApiEndpoints.Users.UserTenants(userId), ct);

    /// <summary>Tenant içindeki kullanıcı membership durumunu günceller (Suspend/Remove).</summary>
    public Task<ApiResult<bool>> UpdateMembershipStatusAsync(
        Guid userId, Guid tenantId,
        UpdateMembershipStatusRequest req,
        CancellationToken ct = default) =>
        PutAsync<UpdateMembershipStatusRequest, bool>(
            ApiEndpoints.Users.MembershipStatus(userId, tenantId), req, ct);
}
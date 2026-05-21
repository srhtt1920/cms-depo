using CMS.Blazor.Server.Panel.Infrastructure.Http.Base;
using CMS.Blazor.Server.Panel.Models.Auth;
using CMS.Blazor.Server.Panel.Models.Shared;
using CMS.Blazor.Server.Panel.Services.State;

namespace CMS.Blazor.Server.Panel.Infrastructure.ApiClients.Auth;

/// <summary>
/// Authentication ve profil işlemleri.
/// Endpointler: <see cref="ApiEndpoints.Auth"/>
/// </summary>
public sealed class AuthApiClient(IHttpClientFactory factory, ApplicationState appState)
    : ApiClientBase(factory, appState)
{
    // ── Login / Token ──────────────────────────────────────────────────────

    public Task<ApiResult<LoginResponse>> LoginAsync(
        LoginRequest req, CancellationToken ct = default) =>
        PostAsync<LoginRequest, LoginResponse>(ApiEndpoints.Auth.Login, req, auth: false, ct: ct);

    public Task<ApiResult<LoginResponse>> RefreshTokenAsync(
        string refreshToken, CancellationToken ct = default) =>
        PostAsync<RefreshTokenRequest, LoginResponse>(
            ApiEndpoints.Auth.Refresh, new RefreshTokenRequest(refreshToken), auth: false, ct: ct);

    public Task<ApiResult<bool>> LogoutAsync(
        string? refreshToken = null, CancellationToken ct = default) =>
        PostAsync<LogoutRequest, bool>(
            ApiEndpoints.Auth.Logout, new LogoutRequest(refreshToken), ct: ct);

    public Task<ApiResult<SelectTenantResponse>> SelectTenantAsync(
        Guid tenantId, CancellationToken ct = default) =>
        PostAsync<object, SelectTenantResponse>(
            ApiEndpoints.Auth.SelectTenant, new { TenantId = tenantId }, ct: ct);

    // ── Profile ────────────────────────────────────────────────────────────

    public Task<ApiResult<UpdateProfileResponse>> GetProfileAsync(
        CancellationToken ct = default) =>
        GetAsync<UpdateProfileResponse>(ApiEndpoints.Auth.Me, ct);

    public Task<ApiResult<UpdateProfileResponse>> UpdateProfileAsync(
        UpdateProfileRequest req, CancellationToken ct = default) =>
        PutAsync<UpdateProfileRequest, UpdateProfileResponse>(ApiEndpoints.Auth.Me, req, ct);

    public Task<ApiResult<bool>> ChangePasswordAsync(
        ChangePasswordRequest req, CancellationToken ct = default) =>
        PutAsync<ChangePasswordRequest, bool>(ApiEndpoints.Auth.MePassword, req, ct);

    // ── Permissions ────────────────────────────────────────────────────────

    public Task<ApiResult<PermissionTreeDto>> GetPermissionsAsync(
        CancellationToken ct = default) =>
        GetAsync<PermissionTreeDto>(ApiEndpoints.Auth.MyPermissions, ct);

    public Task<ApiResult<PermissionTreeDto>> GetPermissionsAsync(
        string token, CancellationToken ct = default) =>
        GetAsync<PermissionTreeDto>(ApiEndpoints.Auth.MyPermissions, ct, tokenOverride: token);

    // ── Password Reset ──────────────────────────────────────────────────────

    public Task<ApiResult<bool>> ForgotPasswordAsync(
        ForgotPasswordRequest req, CancellationToken ct = default) =>
        PostAsync<ForgotPasswordRequest, bool>(
            ApiEndpoints.Auth.ForgotPassword, req, auth: false, ct: ct);

    public Task<ApiResult<bool>> ResetPasswordAsync(
        ResetPasswordRequest req, CancellationToken ct = default) =>
        PostAsync<ResetPasswordRequest, bool>(
            ApiEndpoints.Auth.ResetPassword, req, auth: false, ct: ct);

    // ── Two-Factor Authentication ───────────────────────────────────────────

    public Task<ApiResult<TwoFactorSetupResponse>> TwoFactorSetupAsync(
        CancellationToken ct = default) =>
        PostAsync<object, TwoFactorSetupResponse>(
            ApiEndpoints.Auth.TwoFactorSetup, new { }, ct: ct);

    public Task<ApiResult<TwoFactorVerifyResponse>> TwoFactorVerifyAsync(
        TwoFactorVerifyRequest req, CancellationToken ct = default) =>
        PostAsync<TwoFactorVerifyRequest, TwoFactorVerifyResponse>(
            ApiEndpoints.Auth.TwoFactorVerify, req, auth: false, ct: ct);

    public Task<ApiResult<bool>> TwoFactorDisableAsync(
        TwoFactorDisableRequest req, CancellationToken ct = default) =>
        PostAsync<TwoFactorDisableRequest, bool>(
            ApiEndpoints.Auth.TwoFactorDisable, req, ct: ct);

    public Task<ApiResult<bool>> TwoFactorResendAsync(
        TwoFactorResendRequest req, CancellationToken ct = default) =>
        PostAsync<TwoFactorResendRequest, bool>(
            ApiEndpoints.Auth.TwoFactorResend, req, auth: false, ct: ct);
}

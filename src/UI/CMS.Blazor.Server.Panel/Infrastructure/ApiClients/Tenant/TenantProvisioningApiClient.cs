using CMS.Blazor.Server.Panel.Infrastructure.Http.Base;
using CMS.Blazor.Server.Panel.Models.Shared;
using CMS.Blazor.Server.Panel.Models.Tenant;
using CMS.Blazor.Server.Panel.Services.State;

namespace CMS.Blazor.Server.Panel.Infrastructure.ApiClients.Tenant;

/// <summary>
/// Tenant provisioning, status yönetimi ve feature flag işlemleri.
/// Endpointler: <see cref="ApiEndpoints.Tenants"/> (Provisioning + Status bölümü)
/// </summary>
public sealed class TenantProvisioningApiClient(IHttpClientFactory factory, ApplicationState appState)
    : ApiClientBase(factory, appState)
{
    // ── Provisioning ────────────────────────────────────────────────────────

    /// <summary>
    /// Yeni tenant oluşturur ve default data provisioning başlatır.
    /// Tek çağrıyla: tenant + roller + izinler + dil + ana sayfa + navigasyon.
    /// </summary>
    public Task<ApiResult<TenantProvisionResponse>> ProvisionTenantAsync(
        TenantProvisionRequest req, CancellationToken ct = default) =>
        PostAsync<TenantProvisionRequest, TenantProvisionResponse>(
            ApiEndpoints.Tenants.Provision, req, ct: ct);

    /// <summary>
    /// Provisioning ilerlemesini polling ile kontrol eder.
    /// Wizard step 4'te kullanılır.
    /// </summary>
    public Task<ApiResult<TenantProvisioningStatusDto>> GetProvisioningStatusAsync(
        Guid tenantId, CancellationToken ct = default) =>
        GetAsync<TenantProvisioningStatusDto>(
            ApiEndpoints.Tenants.ProvisioningStatus(tenantId), ct);

    // ── Tenant Status ───────────────────────────────────────────────────────

    /// <summary>
    /// Tenant durumunu günceller: Active → Suspended → Archived vb.
    /// SuperAdmin kullanır.
    /// </summary>
    public Task<ApiResult<bool>> SetTenantStatusAsync(
        Guid tenantId, SetTenantStatusRequest req, CancellationToken ct = default) =>
        PutAsync<SetTenantStatusRequest, bool>(
            ApiEndpoints.Tenants.Status(tenantId), req, ct);

    // ── Feature Flags ───────────────────────────────────────────────────────

    /// <summary>Tenant'ın mevcut feature flag'lerini döner.</summary>
    public Task<ApiResult<TenantFeatureFlagsDto>> GetFeatureFlagsAsync(
        Guid tenantId, CancellationToken ct = default) =>
        GetAsync<TenantFeatureFlagsDto>(
            ApiEndpoints.Tenants.FeatureFlags(tenantId), ct);

    /// <summary>Bir veya birden fazla feature flag'i günceller.</summary>
    public Task<ApiResult<bool>> UpdateFeatureFlagsAsync(
        Guid tenantId, UpdateTenantFeatureFlagsRequest req, CancellationToken ct = default) =>
        PutAsync<UpdateTenantFeatureFlagsRequest, bool>(
            ApiEndpoints.Tenants.FeatureFlags(tenantId), req, ct);
}
using CMS.Blazor.Server.Panel.Infrastructure.Http.Base;
using CMS.Blazor.Server.Panel.Models.Shared;
using CMS.Blazor.Server.Panel.Models.Tenant;
using CMS.Blazor.Server.Panel.Services.State;

namespace CMS.Blazor.Server.Panel.Infrastructure.ApiClients.Tenant;

/// <summary>
/// Tenant CRUD ve dil ayarları.
/// Endpointler: <see cref="ApiEndpoints.Tenants"/>
/// </summary>
public sealed class TenantApiClient(IHttpClientFactory factory, ApplicationState appState)
    : ApiClientBase(factory, appState)
{
    public Task<ApiResult<List<TenantListDto>>> GetTenantsAsync(
        CancellationToken ct = default) =>
        GetAsync<List<TenantListDto>>(ApiEndpoints.Tenants.Base, ct);

    public Task<ApiResult<TenantSettingsDto>> GetTenantSettingsAsync(
        Guid id, CancellationToken ct = default) =>
        GetAsync<TenantSettingsDto>(ApiEndpoints.Tenants.Settings(id), ct);

    public Task<ApiResult<CreateTenantResponse>> CreateTenantAsync(
        CreateTenantRequest req, CancellationToken ct = default) =>
        PostAsync<CreateTenantRequest, CreateTenantResponse>(
            ApiEndpoints.Tenants.Base, req, ct: ct);

    public Task<ApiResult<bool>> UpdateTenantAsync(
        Guid id, UpdateTenantNameRequest req, CancellationToken ct = default) =>
        PutAsync<UpdateTenantNameRequest, bool>(ApiEndpoints.Tenants.ById(id), req, ct);

    public Task<ApiResult<bool>> UpdateTenantLanguagesAsync(
        Guid id, UpdateLanguagesApiRequest req, CancellationToken ct = default) =>
        PutAsync<UpdateLanguagesApiRequest, bool>(ApiEndpoints.Tenants.Languages(id), req, ct);

    public Task<ApiResult<bool>> DeactivateTenantAsync(
        Guid id, CancellationToken ct = default) =>
        DeleteAsync<bool>(ApiEndpoints.Tenants.Delete(id), ct);
}

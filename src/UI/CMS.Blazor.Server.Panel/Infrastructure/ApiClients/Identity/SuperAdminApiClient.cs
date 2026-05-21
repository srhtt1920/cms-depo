using CMS.Blazor.Server.Panel.Infrastructure.Http.Base;
using CMS.Blazor.Server.Panel.Models.Audit;
using CMS.Blazor.Server.Panel.Models.Shared;
using CMS.Blazor.Server.Panel.Models.SuperAdmin;
using CMS.Blazor.Server.Panel.Models.Tenant;
using CMS.Blazor.Server.Panel.Services.State;

namespace CMS.Blazor.Server.Panel.Infrastructure.ApiClients.Identity;

/// <summary>
/// SuperAdmin işlemleri: yönetici atama, tenant bakım/aktif kontrolü, kullanıcı transfer.
/// Endpointler: <see cref="ApiEndpoints.SuperAdmin"/>
/// </summary>
public sealed class SuperAdminApiClient(IHttpClientFactory factory, ApplicationState appState)
    : ApiClientBase(factory, appState)
{
    // ── Admin Management ───────────────────────────────────────────────────

    public Task<ApiResult<List<SuperAdminListDto>>> GetSuperAdminsAsync(
        CancellationToken ct = default) =>
        GetAsync<List<SuperAdminListDto>>(ApiEndpoints.SuperAdmin.Admins, ct);

    public Task<ApiResult<bool>> GrantSuperAdminAsync(
        GrantSuperAdminRequest req, CancellationToken ct = default) =>
        PostAsync<GrantSuperAdminRequest, bool>(ApiEndpoints.SuperAdmin.Grant, req, ct: ct);

    public Task<ApiResult<bool>> RevokeSuperAdminAsync(
        RevokeSuperAdminRequest req, CancellationToken ct = default) =>
        DeleteWithBodyAsync<RevokeSuperAdminRequest, bool>(
            ApiEndpoints.SuperAdmin.Revoke, req, ct);

    public Task<ApiResult<AuditLogPagedDto>> GetSuperAdminAuditAsync(
        int pageIndex = 0, int pageSize = 50, CancellationToken ct = default)
    {
        var q = BuildQuery(("pageIndex", pageIndex), ("pageSize", pageSize));
        return GetAsync<AuditLogPagedDto>($"{ApiEndpoints.SuperAdmin.Audit}{q}", ct);
    }

    // ── Tenant Maintenance ─────────────────────────────────────────────────

    public Task<ApiResult<TenantDetailDto>> GetTenantDetailAsync(
        Guid tenantId, CancellationToken ct = default) =>
        GetAsync<TenantDetailDto>(ApiEndpoints.SuperAdmin.TenantDetail(tenantId), ct);

    public Task<ApiResult<bool>> SetTenantActiveAsync(
        Guid tenantId, SetTenantActiveRequest req, CancellationToken ct = default) =>
        PutAsync<SetTenantActiveRequest, bool>(
            ApiEndpoints.SuperAdmin.TenantActive(tenantId), req, ct);

    public Task<ApiResult<TenantMaintenanceDto>> GetTenantMaintenanceAsync(
        Guid tenantId, CancellationToken ct = default) =>
        GetAsync<TenantMaintenanceDto>(ApiEndpoints.SuperAdmin.TenantMaintenance(tenantId), ct);

    public Task<ApiResult<bool>> SetTenantMaintenanceAsync(
        Guid tenantId, SetMaintenanceModeRequest req, CancellationToken ct = default) =>
        PutAsync<SetMaintenanceModeRequest, bool>(
            ApiEndpoints.SuperAdmin.TenantMaintenance(tenantId), req, ct);

    // ── User Transfer ──────────────────────────────────────────────────────

    public Task<ApiResult<UserTransferPreviewDto>> GetTransferPreviewAsync(
        Guid userId, Guid sourceTenantId, CancellationToken ct = default) =>
        GetAsync<UserTransferPreviewDto>(
            $"{ApiEndpoints.SuperAdmin.TransferPreview(userId)}{BuildQuery(("sourceTenantId", sourceTenantId))}", ct);

    public Task<ApiResult<TransferUserResponse>> TransferUserAsync(
        TransferUserRequest req, CancellationToken ct = default) =>
        PostAsync<TransferUserRequest, TransferUserResponse>(
            ApiEndpoints.SuperAdmin.Transfer, req, ct: ct);

    public Task<ApiResult<bool>> AssignUserToTenantAsync(
        Guid tenantId, AssignUserToTenantRequest req, CancellationToken ct = default) =>
        PostAsync<AssignUserToTenantRequest, bool>(
            ApiEndpoints.SuperAdmin.AssignUser(tenantId), req, ct: ct);

    public Task<ApiResult<bool>> RemoveUserFromTenantAsync(
        Guid tenantId, Guid userId, CancellationToken ct = default) =>
        DeleteAsync<bool>(ApiEndpoints.SuperAdmin.RemoveUser(tenantId, userId), ct);
}

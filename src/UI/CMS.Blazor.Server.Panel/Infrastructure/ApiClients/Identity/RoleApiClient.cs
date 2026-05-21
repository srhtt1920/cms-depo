using CMS.Blazor.Server.Panel.Infrastructure.Http.Base;
using CMS.Blazor.Server.Panel.Models.Role;
using CMS.Blazor.Server.Panel.Models.Shared;
using CMS.Blazor.Server.Panel.Services.State;

namespace CMS.Blazor.Server.Panel.Infrastructure.ApiClients.Identity;

/// <summary>
/// Rol CRUD ve atama işlemleri.
/// Endpointler: <see cref="ApiEndpoints.Roles"/>
/// </summary>
public sealed class RoleApiClient(IHttpClientFactory factory, ApplicationState appState)
    : ApiClientBase(factory, appState)
{
    public Task<ApiResult<List<RoleListDto>>> GetRolesAsync(
        CancellationToken ct = default) =>
        GetAsync<List<RoleListDto>>(ApiEndpoints.Roles.Base, ct);

    public Task<ApiResult<RoleDetailDto>> GetRoleByIdAsync(
        Guid id, CancellationToken ct = default) =>
        GetAsync<RoleDetailDto>(ApiEndpoints.Roles.ById(id), ct);

    public Task<ApiResult<CreateRoleApiResponse>> CreateRoleAsync(
        CreateRoleApiRequest req, CancellationToken ct = default) =>
        PostAsync<CreateRoleApiRequest, CreateRoleApiResponse>(
            ApiEndpoints.Roles.Base, req, ct: ct);

    public Task<ApiResult<RoleListDto>> UpdateRoleAsync(
        Guid roleId, UpdateRoleApiRequest req, CancellationToken ct = default) =>
        PutAsync<UpdateRoleApiRequest, RoleListDto>(ApiEndpoints.Roles.ById(roleId), req, ct);

    public Task<ApiResult<bool>> DeleteRoleAsync(
        Guid roleId, CancellationToken ct = default) =>
        DeleteAsync<bool>(ApiEndpoints.Roles.ById(roleId), ct);

    public Task<ApiResult<bool>> AssignRoleAsync(
        Guid roleId, AssignRoleApiRequest req, CancellationToken ct = default) =>
        PostAsync<AssignRoleApiRequest, bool>(
            ApiEndpoints.Roles.Assign(roleId), req, ct: ct);

    /// <remarks>
    /// NOTE: HTTP DELETE with body kullanılır çünkü HttpClient.DeleteAsync body desteklemez.
    /// </remarks>
    public Task<ApiResult<bool>> RevokeRoleAsync(
        Guid roleId, RevokeRoleApiRequest req, CancellationToken ct = default) =>
        DeleteWithBodyAsync<RevokeRoleApiRequest, bool>(
            ApiEndpoints.Roles.Revoke(roleId), req, ct);
}

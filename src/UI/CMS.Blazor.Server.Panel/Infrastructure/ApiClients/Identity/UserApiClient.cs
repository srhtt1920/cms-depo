using CMS.Blazor.Server.Panel.Infrastructure.Http.Base;
using CMS.Blazor.Server.Panel.Models.Shared;
using CMS.Blazor.Server.Panel.Models.User;
using CMS.Blazor.Server.Panel.Services.State;

namespace CMS.Blazor.Server.Panel.Infrastructure.ApiClients.Identity;

/// <summary>
/// Kullanıcı CRUD, rol atama ve aktivite geçmişi.
/// Endpointler: <see cref="ApiEndpoints.Users"/>
/// </summary>
public sealed class UserApiClient(IHttpClientFactory factory, ApplicationState appState)
    : ApiClientBase(factory, appState)
{
    public Task<ApiResult<List<UserListDto>>> GetUsersAsync(
        CancellationToken ct = default) =>
        GetAsync<List<UserListDto>>(ApiEndpoints.Users.Base, ct);

    public Task<ApiResult<UserListDto>> GetUserByIdAsync(
        Guid userId, CancellationToken ct = default) =>
        GetAsync<UserListDto>(ApiEndpoints.Users.ById(userId), ct);

    public Task<ApiResult<CreateUserResponse>> CreateUserAsync(
        CreateUserApiRequest req, CancellationToken ct = default) =>
        PostAsync<CreateUserApiRequest, CreateUserResponse>(
            ApiEndpoints.Users.Base, req, ct: ct);

    public Task<ApiResult<UserListDto>> UpdateUserRolesAsync(
        Guid userId, UpdateRolesApiRequest req, CancellationToken ct = default) =>
        PutAsync<UpdateRolesApiRequest, UserListDto>(ApiEndpoints.Users.Roles(userId), req, ct);

    public Task<ApiResult<bool>> SetUserActiveAsync(
        Guid userId, bool isActive, CancellationToken ct = default) =>
        PutAsync<SetActiveApiRequest, bool>(
            ApiEndpoints.Users.Active(userId), new SetActiveApiRequest(isActive), ct);

    public Task<ApiResult<bool>> DeleteUserAsync(
        Guid userId, CancellationToken ct = default) =>
        DeleteAsync<bool>(ApiEndpoints.Users.Delete(userId), ct);

    public Task<ApiResult<UserActivityPageDto>> GetUserActivityAsync(
        Guid userId, int pageIndex = 0, int pageSize = 30,
        CancellationToken ct = default)
    {
        var q = BuildQuery(("pageIndex", pageIndex), ("pageSize", pageSize));
        return GetAsync<UserActivityPageDto>($"{ApiEndpoints.Users.Activity(userId)}{q}", ct);
    }
}

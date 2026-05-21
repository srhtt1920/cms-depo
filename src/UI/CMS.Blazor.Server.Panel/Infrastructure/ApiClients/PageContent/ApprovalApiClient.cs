using CMS.Blazor.Server.Panel.Infrastructure.Http.Base;
using CMS.Blazor.Server.Panel.Models.Approval;
using CMS.Blazor.Server.Panel.Models.Shared;
using CMS.Blazor.Server.Panel.Services.State;

namespace CMS.Blazor.Server.Panel.Infrastructure.ApiClients.PageContent;

/// <summary>
/// İçerik onay akışı işlemleri.
/// Endpointler: <see cref="ApiEndpoints.Approvals"/>
/// </summary>
public sealed class ApprovalApiClient(IHttpClientFactory factory, ApplicationState appState)
    : ApiClientBase(factory, appState)
{
    public Task<ApiResult<ApprovalPagedDto>> GetApprovalsAsync(
        string? status = "PendingApproval",
        int pageIndex = 0, int pageSize = 20,
        CancellationToken ct = default)
    {
        var q = BuildQuery(
            ("status", status),
            ("pageIndex", pageIndex),
            ("pageSize", pageSize));
        return GetAsync<ApprovalPagedDto>($"{ApiEndpoints.Approvals.Base}{q}", ct);
    }

    public Task<ApiResult<ApprovalDetailDto>> GetApprovalByIdAsync(
        Guid id, CancellationToken ct = default) =>
        GetAsync<ApprovalDetailDto>(ApiEndpoints.Approvals.ById(id), ct);

    public Task<ApiResult<bool>> ApproveContentAsync(
        Guid id, string? comment = null, CancellationToken ct = default) =>
        PostAsync<ApproveRequest, bool>(
            ApiEndpoints.Approvals.Approve(id), new ApproveRequest(comment), ct: ct);

    public Task<ApiResult<bool>> RejectContentAsync(
        Guid id, string reason, CancellationToken ct = default) =>
        PostAsync<RejectRequest, bool>(
            ApiEndpoints.Approvals.Reject(id), new RejectRequest(reason), ct: ct);
}

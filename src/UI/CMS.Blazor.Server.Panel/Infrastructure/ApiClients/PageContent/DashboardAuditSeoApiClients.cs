using CMS.Blazor.Server.Panel.Infrastructure.Http.Base;
using CMS.Blazor.Server.Panel.Models.Audit;
using CMS.Blazor.Server.Panel.Models.Content;
using CMS.Blazor.Server.Panel.Models.Dashboard;
using CMS.Blazor.Server.Panel.Models.Shared;
using CMS.Blazor.Server.Panel.Services.State;

namespace CMS.Blazor.Server.Panel.Infrastructure.ApiClients.PageContent;

/// <summary>
/// Dashboard istatistikleri ve takvim verisi.
/// Endpointler: <see cref="ApiEndpoints.Dashboard"/>
/// </summary>
public sealed class DashboardApiClient(IHttpClientFactory factory, ApplicationState appState)
    : ApiClientBase(factory, appState)
{
    public Task<ApiResult<DashboardStatsDto>> GetDashboardStatsAsync(
        CancellationToken ct = default) =>
        GetAsync<DashboardStatsDto>(ApiEndpoints.Dashboard.Stats, ct);

    public Task<ApiResult<List<ScheduleCalendarItemDto>>> GetScheduleCalendarAsync(
        DateTime from, DateTime to, CancellationToken ct = default)
    {
        var q = BuildQuery(("from", from.ToString("O")), ("to", to.ToString("O")));
        return GetAsync<List<ScheduleCalendarItemDto>>(
            $"{ApiEndpoints.Dashboard.Calendar}{q}", ct);
    }
}

/// <summary>
/// Audit log sorguları.
/// Endpointler: <see cref="ApiEndpoints.AuditLogs"/>
/// </summary>
public sealed class AuditApiClient(IHttpClientFactory factory, ApplicationState appState)
    : ApiClientBase(factory, appState)
{
    public Task<ApiResult<AuditLogPagedDto>> GetAuditLogsAsync(
        int pageIndex = 0, int pageSize = 50,
        string? entityType = null, string? action = null, Guid? userId = null,
        CancellationToken ct = default)
    {
        var q = BuildQuery(
            ("pageIndex", pageIndex),
            ("pageSize", pageSize),
            ("entityType", entityType),
            ("action", action),
            ("userId", userId));
        return GetAsync<AuditLogPagedDto>($"{ApiEndpoints.AuditLogs.Base}{q}", ct);
    }
}

/// <summary>
/// SEO meta veri sorguları.
/// Endpointler: <see cref="ApiEndpoints.Seo"/>
/// </summary>
public sealed class SeoApiClient(IHttpClientFactory factory, ApplicationState appState)
    : ApiClientBase(factory, appState)
{
    public Task<ApiResult<ContentMetaDto>> GetContentMetaAsync(
        string slug, string lang, CancellationToken ct = default) =>
        GetAsync<ContentMetaDto>(
            $"{ApiEndpoints.Seo.ContentMeta(slug)}{BuildQuery(("lang", lang))}", ct);
}

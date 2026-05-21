using CMS.Blazor.Server.Panel.Infrastructure.Http.Base;
using CMS.Blazor.Server.Panel.Models.Page;
using CMS.Blazor.Server.Panel.Models.Shared;
using CMS.Blazor.Server.Panel.Services.State;

namespace CMS.Blazor.Server.Panel.Infrastructure.ApiClients.PageContent;

/// <summary>
/// Sayfa ağacı, CRUD ve navigasyon işlemleri.
/// Endpointler: <see cref="ApiEndpoints.Pages"/>
/// </summary>
public sealed class PageApiClient(IHttpClientFactory factory, ApplicationState appState)
    : ApiClientBase(factory, appState)
{
    // ── Tree ───────────────────────────────────────────────────────────────

    public Task<ApiResult<List<PageTreeNodeDto>>> GetPageTreeAsync(
        bool includeInactive = false, CancellationToken ct = default) =>
        GetAsync<List<PageTreeNodeDto>>(
            $"{ApiEndpoints.Pages.Tree}{BuildQuery(("includeInactive", includeInactive))}", ct);

    public Task<ApiResult<List<PageTreeDetailNodeDto>>> GetPageTreeDetailAsync(
        bool includeInactive = false, string languageCode = "tr",
        CancellationToken ct = default) =>
        GetAsync<List<PageTreeDetailNodeDto>>(
            $"{ApiEndpoints.Pages.TreeDetail}{BuildQuery(("languageCode", languageCode), ("includeInactive", includeInactive))}", ct);

    // ── Single ─────────────────────────────────────────────────────────────

    public Task<ApiResult<PageDetailDto>> GetPageByIdAsync(
        Guid id, CancellationToken ct = default) =>
        GetAsync<PageDetailDto>(ApiEndpoints.Pages.ById(id), ct);

    public Task<ApiResult<List<object>>> GetPageBreadcrumbAsync(
        Guid id, string languageCode = "tr", CancellationToken ct = default) =>
        GetAsync<List<object>>(
            $"{ApiEndpoints.Pages.Breadcrumb(id)}{BuildQuery(("languageCode", languageCode))}", ct);

    // ── CRUD ───────────────────────────────────────────────────────────────

    public Task<ApiResult<CreatePageResponse>> CreatePageAsync(
        CreatePageRequest req, CancellationToken ct = default) =>
        PostAsync<CreatePageRequest, CreatePageResponse>(ApiEndpoints.Pages.Base, req, ct: ct);

    public Task<ApiResult<bool>> DeletePageAsync(
        Guid id, CancellationToken ct = default) =>
        DeleteAsync<bool>(ApiEndpoints.Pages.Delete(id), ct);

    // ── Translations ───────────────────────────────────────────────────────

    public Task<ApiResult<bool>> UpsertPageTranslationAsync(
        Guid id, UpsertPageTranslationRequest req, CancellationToken ct = default) =>
        PutAsync<UpsertPageTranslationRequest, bool>(
            ApiEndpoints.Pages.Translations(id), req, ct);

    // ── Settings / State ───────────────────────────────────────────────────

    public Task<ApiResult<bool>> UpdatePageSettingsAsync(
        Guid pageId, UpdatePageSettingsRequest req, CancellationToken ct = default) =>
        PutAsync<UpdatePageSettingsRequest, bool>(
            ApiEndpoints.Pages.Settings(pageId), req, ct);

    public Task<ApiResult<bool>> SetPageActiveAsync(
        Guid pageId, bool isActive, CancellationToken ct = default) =>
        PutAsync<SetPageActiveApiRequest, bool>(
            ApiEndpoints.Pages.Active(pageId), new SetPageActiveApiRequest(isActive), ct);

    public Task<ApiResult<bool>> MovePageAsync(
        Guid id, MovePageApiRequest req, CancellationToken ct = default) =>
        PutAsync<MovePageApiRequest, bool>(ApiEndpoints.Pages.Move(id), req, ct);

    public Task<ApiResult<bool>> LinkPageContentAsync(
        Guid id, Guid? contentId, CancellationToken ct = default) =>
        PutAsync<LinkContentApiRequest, bool>(
            ApiEndpoints.Pages.ContentLink(id), new LinkContentApiRequest(contentId), ct);

    // ── Lock ───────────────────────────────────────────────────────────────

    public Task<ApiResult<ContentLockDto>> GetPageLockAsync(
        Guid pageId, CancellationToken ct = default) =>
        GetAsync<ContentLockDto>(ApiEndpoints.Pages.Lock(pageId), ct);

    public Task<ApiResult<bool>> SetPageLockAsync(
        Guid pageId, SetContentLockRequest req, CancellationToken ct = default) =>
        PutAsync<SetContentLockRequest, bool>(ApiEndpoints.Pages.Lock(pageId), req, ct);
}

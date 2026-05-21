using CMS.Blazor.Server.Panel.Infrastructure.Http.Base;
using CMS.Blazor.Server.Panel.Models.Content;
using CMS.Blazor.Server.Panel.Models.Shared;
using CMS.Blazor.Server.Panel.Services.State;

namespace CMS.Blazor.Server.Panel.Infrastructure.ApiClients.PageContent;

/// <summary>
/// İçerik (content) CRUD, bulk, versiyon, schedule, section ve block işlemleri.
/// Endpointler: <see cref="ApiEndpoints.Contents"/>
/// </summary>
public sealed class ContentApiClient(IHttpClientFactory factory, ApplicationState appState)
    : ApiClientBase(factory, appState)
{
    // ── List / Query ───────────────────────────────────────────────────────

    /// <remarks>
    /// BUG FIX: API ListContentsResponse wrapper içinde Paginate döner;
    /// doğrudan ContentPagedDto değil.
    /// </remarks>
    public async Task<ApiResult<ContentPagedDto>> GetContentsAsync(
        int pageIndex = 0, int pageSize = 25,
        string? status = null, string? search = null, string? contentType = null,
        CancellationToken ct = default)
    {
        var q = BuildQuery(
            ("pageIndex", pageIndex),
            ("pageSize", pageSize),
            ("status", status),
            ("search", search),
            ("contentType", contentType));

        var r = await GetAsync<ListContentsResponse>($"{ApiEndpoints.Contents.Base}{q}", ct);
        if (!r.IsSuccess) return ApiResult<ContentPagedDto>.Fail(r.Error!.Code, r.Error.Message);
        return ApiResult<ContentPagedDto>.Ok(r.Data!.Data);
    }

    public Task<ApiResult<ContentDto>> GetContentBySlugAsync(
        string slug, CancellationToken ct = default) =>
        GetAsync<ContentDto>(ApiEndpoints.Contents.BySlug(slug), ct);

    public Task<ApiResult<ContentDetailDto>> GetContentByIdAsync(
        Guid id, CancellationToken ct = default) =>
        GetAsync<ContentDetailDto>(ApiEndpoints.Contents.ById(id), ct);

    public Task<ApiResult<ContentWithSectionsDto>> GetContentWithSectionsAsync(
        Guid id, CancellationToken ct = default) =>
        GetAsync<ContentWithSectionsDto>(ApiEndpoints.Contents.Sections(id), ct);

    // ── CRUD ───────────────────────────────────────────────────────────────

    public Task<ApiResult<CreateContentApiResponse>> CreateContentAsync(
        CreateContentApiRequest req, CancellationToken ct = default) =>
        PostAsync<CreateContentApiRequest, CreateContentApiResponse>(
            ApiEndpoints.Contents.Base, req, ct: ct);

    public Task<ApiResult<bool>> UpdateContentAsync(
        Guid id, UpdateContentApiRequest req, CancellationToken ct = default) =>
        PutAsync<UpdateContentApiRequest, bool>(ApiEndpoints.Contents.Update(id), req, ct);

    public Task<ApiResult<bool>> SoftDeleteContentAsync(
        Guid id, CancellationToken ct = default) =>
        DeleteAsync<bool>(ApiEndpoints.Contents.SoftDelete(id), ct);

    public Task<ApiResult<CreateContentApiResponse>> DuplicateContentAsync(
        Guid id, string newSlug, CancellationToken ct = default) =>
        PostAsync<DuplicateContentRequest, CreateContentApiResponse>(
            ApiEndpoints.Contents.Duplicate(id), new DuplicateContentRequest(newSlug), ct: ct);

    // ── Translations ───────────────────────────────────────────────────────

    public Task<ApiResult<bool>> AddTranslationAsync(
        Guid id, AddTranslationApiRequest req, CancellationToken ct = default) =>
        PostAsync<AddTranslationApiRequest, bool>(
            ApiEndpoints.Contents.Translations(id), req, ct: ct);

    // ── Workflow ───────────────────────────────────────────────────────────

    public Task<ApiResult<bool>> PublishContentAsync(
        Guid id, CancellationToken ct = default) =>
        PostAsync<object, bool>(ApiEndpoints.Contents.Publish(id), new { }, ct: ct);

    public Task<ApiResult<bool>> UnpublishContentAsync(
        Guid id, CancellationToken ct = default) =>
        PostAsync<object, bool>(ApiEndpoints.Contents.Unpublish(id), new { }, ct: ct);

    public Task<ApiResult<bool>> ArchiveContentAsync(
        Guid id, CancellationToken ct = default) =>
        PostAsync<object, bool>(ApiEndpoints.Contents.Archive(id), new { }, ct: ct);

    // ── Bulk ───────────────────────────────────────────────────────────────

    /// <remarks>
    /// BUG FIX: Action artık string değil BulkActionType enum olarak gönderilir.
    /// </remarks>
    public Task<ApiResult<BulkActionResult>> BulkActionAsync(
        BulkActionRequest req, CancellationToken ct = default) =>
        PostAsync<BulkActionRequest, BulkActionResult>(
            ApiEndpoints.Contents.Bulk, req, ct: ct);

    public Task<ApiResult<bool>> BulkLockContentsAsync(
        BulkLockRequest req, CancellationToken ct = default) =>
        PostAsync<BulkLockRequest, bool>(ApiEndpoints.Contents.BulkLock, req, ct: ct);

    // ── Trash ──────────────────────────────────────────────────────────────

    /// <remarks>BUG FIX: List&lt;TrashItemDto&gt; — ContentSummaryDto değil.</remarks>
    public Task<ApiResult<List<TrashItemDto>>> GetTrashAsync(
        CancellationToken ct = default) =>
        GetAsync<List<TrashItemDto>>(ApiEndpoints.Contents.Trash, ct);

    /// <remarks>BUG FIX: /trash/{id}/hard — /contents/{id}/hard değil.</remarks>
    public Task<ApiResult<bool>> HardDeleteContentAsync(
        Guid id, CancellationToken ct = default) =>
        DeleteAsync<bool>(ApiEndpoints.Contents.TrashHardDelete(id), ct);

    /// <remarks>BUG FIX: /trash/{id}/restore — /contents/{id}/restore değil.</remarks>
    public Task<ApiResult<bool>> RestoreContentAsync(
        Guid id, CancellationToken ct = default) =>
        PostAsync<object, bool>(ApiEndpoints.Contents.TrashRestore(id), new { }, ct: ct);

    public Task<ApiResult<ContentCompletionDto>> GetCompletionAsync(
        Guid id, CancellationToken ct = default) =>
        GetAsync<ContentCompletionDto>(ApiEndpoints.Contents.TrashCompletion(id), ct);

    // ── Versions ───────────────────────────────────────────────────────────

    public Task<ApiResult<List<ContentVersionDto>>> GetVersionsAsync(
        Guid id, CancellationToken ct = default) =>
        GetAsync<List<ContentVersionDto>>(ApiEndpoints.Contents.Versions(id), ct);

    public Task<ApiResult<bool>> RestoreVersionAsync(
        Guid id, int versionNumber, CancellationToken ct = default) =>
        PostAsync<object, bool>(
            ApiEndpoints.Contents.RestoreVersion(id, versionNumber), new { }, ct: ct);

    // ── Schedule ───────────────────────────────────────────────────────────

    public Task<ApiResult<bool>> SetScheduleAsync(
        Guid id, SetScheduleRequest req, CancellationToken ct = default) =>
        PostAsync<SetScheduleRequest, bool>(ApiEndpoints.Contents.Schedule(id), req, ct: ct);

    public Task<ApiResult<bool>> ClearScheduleAsync(
        Guid id, CancellationToken ct = default) =>
        DeleteAsync<bool>(ApiEndpoints.Contents.Schedule(id), ct);

    // ── Lock ───────────────────────────────────────────────────────────────

    public Task<ApiResult<ContentLockDto>> GetContentLockAsync(
        Guid id, CancellationToken ct = default) =>
        GetAsync<ContentLockDto>(ApiEndpoints.Contents.Lock(id), ct);

    public Task<ApiResult<bool>> SetContentLockAsync(
        Guid id, SetContentLockRequest req, CancellationToken ct = default) =>
        PutAsync<SetContentLockRequest, bool>(ApiEndpoints.Contents.Lock(id), req, ct);

    // ── Sections ───────────────────────────────────────────────────────────

    public Task<ApiResult<Guid>> CreateSectionAsync(
        Guid contentId, CreateSectionRequest req, CancellationToken ct = default) =>
        PostAsync<CreateSectionRequest, Guid>(
            ApiEndpoints.Contents.Sections(contentId), req, ct: ct);

    public Task<ApiResult<bool>> UpdateSectionAsync(
        Guid contentId, Guid sectionId, UpdateSectionRequest req, CancellationToken ct = default) =>
        PutAsync<UpdateSectionRequest, bool>(
            ApiEndpoints.Contents.Section(contentId, sectionId), req, ct);

    public Task<ApiResult<bool>> ReorderSectionsAsync(
        Guid contentId, List<SectionOrderItem> items, CancellationToken ct = default) =>
        PutAsync<List<SectionOrderItem>, bool>(
            ApiEndpoints.Contents.SectionsReorder(contentId), items, ct);

    public Task<ApiResult<bool>> DeleteSectionAsync(
        Guid contentId, Guid sectionId, CancellationToken ct = default) =>
        DeleteAsync<bool>(ApiEndpoints.Contents.Section(contentId, sectionId), ct);

    // ── Blocks ─────────────────────────────────────────────────────────────

    public Task<ApiResult<Guid>> UpsertBlockAsync(
        Guid contentId, Guid sectionId, UpsertBlockRequest req, CancellationToken ct = default) =>
        PostAsync<UpsertBlockRequest, Guid>(
            ApiEndpoints.Contents.Blocks(contentId, sectionId), req, ct: ct);

    public Task<ApiResult<bool>> UpsertBlockTranslationAsync(
        Guid contentId, Guid sectionId, Guid blockId,
        UpsertBlockTranslationRequest req, CancellationToken ct = default) =>
        PostAsync<UpsertBlockTranslationRequest, bool>(
            ApiEndpoints.Contents.BlockTranslations(contentId, sectionId, blockId), req, ct: ct);

    public Task<ApiResult<bool>> DeleteBlockAsync(
        Guid contentId, Guid sectionId, Guid blockId, CancellationToken ct = default) =>
        DeleteAsync<bool>(ApiEndpoints.Contents.Block(contentId, sectionId, blockId), ct);
}

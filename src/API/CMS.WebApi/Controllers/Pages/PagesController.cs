using CMS.Application.Features.Pages.CreatePage;
using CMS.Application.Features.Pages.DeletePage;
using CMS.Application.Features.Pages.GetPageBreadcrumb;
using CMS.Application.Features.Pages.GetPageById;
using CMS.Application.Features.Pages.GetPageTree;
using CMS.Application.Features.Pages.GetPageTreeDetail;
using CMS.Application.Features.Pages.LinkPageContent;
using CMS.Application.Features.Pages.MovePage;
using CMS.Application.Features.Pages.SetPageActive;
using CMS.Application.Features.Pages.UpdatePageSettings;
using CMS.Application.Features.Pages.UpsertPageTranslation;
using CMS.WebApi.Controllers.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.WebApi.Controllers.Pages;

[Route("api/pages")]
public sealed class PagesController(ISender sender) : ApiController
{
    // ── Tree endpoint'leri ───────────────────────────────────────────────────

    /// <summary>
    /// Hafif tree — yalnızca Id, ParentId, Order, PageType, ContentType, IsActive, IsVisible, Icon.
    /// Drag-drop, sol menü, hızlı navigasyon için idealdir.
    /// </summary>
    /// <param name="includeInactive">true gönderilirse pasif sayfalar da dahil edilir.</param>
    [HttpGet("tree")]
    [Authorize]
    [ProducesResponseType(typeof(List<PageTreeNodeDto>), 200)]
    public async Task<IActionResult> GetTree(
        [FromQuery] bool includeInactive = false,
        CancellationToken ct = default) =>
        HandleResult(await sender.Send(new GetPageTreeQuery(includeInactive), ct));

    /// <summary>
    /// Detaylı tree — çeviriler, meta, ExternalUrl, LinkedContentId dahil.
    /// Admin düzenleme paneli ve içerik yönetimi için kullanılır.
    /// </summary>
    [HttpGet("tree/detail")]
    [Authorize]
    [ProducesResponseType(typeof(List<PageTreeDetailNodeDto>), 200)]
    public async Task<IActionResult> GetTreeDetail(
        [FromQuery] string languageCode = "tr",
        [FromQuery] bool includeInactive = false,
        CancellationToken ct = default) =>
        HandleResult(await sender.Send(new GetPageTreeDetailQuery(languageCode, includeInactive), ct));

    // ── CRUD ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Tek sayfa detayı — çevirilerin tamamı dahil.
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(PageDetailDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid id,
        CancellationToken ct = default) =>
        HandleResult(await sender.Send(new GetPageByIdQuery(id), ct));

    /// <summary>
    /// Yeni sayfa veya kategori oluşturur.
    /// PageType = Normal ise ContentType zorunludur.
    /// PageType = Category ise ContentType null olmalıdır.
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(CreatePageResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Create(
        [FromBody] CreatePageCommand command,
        CancellationToken ct = default)
    {
        var result = await sender.Send(command, ct);
        if (result.IsFailure) return HandleResult(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    /// <summary>
    /// Soft delete — sayfa ve çevirileri çöp kutusuna taşınır.
    /// Alt sayfaları olan sayfa silinemez, önce alt sayfalar taşınmalıdır.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken ct = default) =>
        HandleResult(await sender.Send(new DeletePageCommand(id), ct));

    // ── Çeviri ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Çeviri ekler veya günceller (upsert).
    /// LanguageCode mevcutsa günceller, yoksa yeni çeviri ekler.
    /// </summary>
    [HttpPut("{id:guid}/translations")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> UpsertTranslation(
        [FromRoute] Guid id,
        [FromBody] UpsertTranslationRequest request,
        CancellationToken ct = default) =>
        HandleResult(await sender.Send(new UpsertPageTranslationCommand(
            id,
            request.LanguageCode,
            request.Title,
            request.LinkName,
            request.Slug,
            request.MetaTitle,
            request.MetaDescription,
            request.MetaKeywords), ct));

    /// <summary>
    /// Sayfa genel ayarlarını günceller (IsActive, IsVisible, Icon, Order, ExternalUrl).
    /// PageLayoutEditor.razor tarafından kullanılır.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSettings(
        [FromRoute] Guid id,
        [FromBody] UpdatePageSettingsRequest request,
        CancellationToken ct = default) =>
        HandleResult(await sender.Send(
            new UpdatePageSettingsCommand(
                id,
                request.IsActive,
                request.IsVisible,
                request.Icon,
                request.Order,
                request.ExternalUrl), ct));


    // ── Tree operasyonları ────────────────────────────────────────────────────

    /// <summary>
    /// Drag-drop sonrası çağrılır.
    /// NewParentId null → root'a taşır.
    /// Döngüsel referans kontrolü yapılır.
    /// </summary>
    [HttpPut("{id:guid}/move")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> Move(
        [FromRoute] Guid id,
        [FromBody] MovePageRequest request,
        CancellationToken ct = default) =>
        HandleResult(await sender.Send(
            new MovePageCommand(id, request.NewParentId, request.NewOrder), ct));

    /// <summary>
    /// Root'tan mevcut sayfaya kadar tam breadcrumb yolu döner.
    /// Depth 0 = root, artan depth = daha derin.
    /// </summary>
    [HttpGet("{id:guid}/breadcrumb")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<BreadcrumbItemDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetBreadcrumb(
        [FromRoute] Guid id,
        [FromQuery] string languageCode = "tr",
        CancellationToken ct = default) =>
        HandleResult(await sender.Send(new GetPageBreadcrumbQuery(id, languageCode), ct));

    // ── Content bağlantısı ────────────────────────────────────────────────────

    /// <summary>
    /// Sayfayı mevcut bir Content kaydıyla ilişkilendirir.
    /// ContentId null gönderilirse bağlantı koparılır.
    /// Yalnızca Normal sayfalarda çalışır — Kategorilerde 422 döner.
    /// </summary>
    [HttpPut("{id:guid}/content-link")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> LinkContent(
        [FromRoute] Guid id,
        [FromBody] LinkContentRequest request,
        CancellationToken ct = default) =>
        HandleResult(await sender.Send(
            new LinkPageContentCommand(id, request.ContentId), ct));
    /// <summary>
    /// Sayfayı aktif veya pasif yapar.
    /// </summary>
    [HttpPut("{id:guid}/active")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> SetActive(
        [FromRoute] Guid id,
        [FromBody] SetPageActiveRequest request,
        CancellationToken ct = default) =>
        HandleResult(await sender.Send(new SetPageActiveCommand(id, request.IsActive), ct));
}

// ── Request DTO'lar ──────────────────────────────────────────────────────────

public sealed record MovePageRequest(
    Guid? NewParentId,
    int NewOrder);

public sealed record SetPageActiveRequest(bool IsActive);

public sealed record UpsertTranslationRequest(
    string LanguageCode,
    string Title,
    string LinkName,
    string Slug,
    string? MetaTitle,
    string? MetaDescription,
    string? MetaKeywords);

public sealed record LinkContentRequest(Guid? ContentId);

public sealed record UpdatePageSettingsRequest(
    bool IsActive,
    bool IsVisible,
    string? Icon,
    int Order,
    string? ExternalUrl);

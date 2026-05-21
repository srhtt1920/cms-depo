using CMS.Application.Features.Contents.AddTranslation;
using CMS.Application.Features.Contents.ArchiveContent;
using CMS.Application.Features.Contents.BulkAction;
using CMS.Application.Features.Contents.CreateContent;
using CMS.Application.Features.Contents.GetContent;
using CMS.Application.Features.Contents.GetContentById;
using CMS.Application.Features.Contents.GetTrashs;
using CMS.Application.Features.Contents.ListContents;
using CMS.Application.Features.Contents.PublishContent;
using CMS.Application.Features.Contents.SoftDelete;
using CMS.Application.Features.Contents.UpdateContent;
using CMS.Domain.Contents;
using CMS.WebApi.Controllers.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.WebApi.Controllers.Contents;

[Route("api/contents")]
public sealed class ContentsController(ISender sender) : ApiController
{
    /// <summary>
    /// Slug ile içerik getirir. Public — X-Tenant-Id header zorunlu.
    /// </summary>
    [HttpGet("{slug}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ContentDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetBySlug(
        [FromRoute] string slug, CancellationToken ct) =>
        HandleResult(await sender.Send(new GetContentQuery(slug), ct));

    /// <summary>
    /// Id ile içerik getirir. Admin paneli için — tüm çeviriler ve schedule dahil.
    /// BUG FIX: Tenant doğrulaması artık uygulanıyor.
    /// </summary>
    [HttpGet("by-id/{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ContentDetailDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid id, CancellationToken ct) =>
        HandleResult(await sender.Send(new GetContentByIdQuery(id), ct));

    /// <summary>
    /// Sayfalı içerik listesi.
    /// Desteklenen filtreler: search, status, contentType, pageIndex, pageSize
    /// </summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(ListContentsResponse), 200)]
    public async Task<IActionResult> List(
        [FromQuery] string? search = null,
        [FromQuery] ContentStatus? status = null,
        [FromQuery] ContentType? contentType = null,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default) =>
        HandleResult(await sender.Send(
            new ListContentsQuery(search, status, contentType, pageIndex, pageSize), ct));

    /// <summary>
    /// Yeni içerik oluşturur.
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(CreateContentResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Create(
        [FromBody] CreateContentCommand command,
        CancellationToken ct = default)
    {
        var result = await sender.Send(command, ct);
        if (result.IsFailure) return HandleResult(result);
        return CreatedAtAction(nameof(GetBySlug),
            new { slug = result.Value.Slug }, result.Value);
    }

    /// <summary>
    /// İçerik çevirisini günceller.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateContentRequest request,
        CancellationToken ct = default)
    {
        var command = new UpdateContentCommand(
            id, request.LanguageCode, request.Title,
            request.Body, request.MetaTitle, request.MetaDescription);
        return HandleResult(await sender.Send(command, ct));
    }

    /// <summary>
    /// Soft delete — içeriği çöp kutusuna taşır.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> SoftDelete(
        [FromRoute] Guid id, CancellationToken ct) =>
        HandleResult(await sender.Send(new SoftDeleteContentCommand(id), ct));

    /// <summary>
    /// İçeriği yayınlar.
    /// </summary>
    [HttpPost("{id:guid}/publish")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Publish(
        [FromRoute] Guid id, CancellationToken ct) =>
        HandleResult(await sender.Send(new PublishContentCommand(id), ct));

    /// <summary>
    /// İçeriği arşivler.
    /// </summary>
    [HttpPost("{id:guid}/archive")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Archive(
        [FromRoute] Guid id, CancellationToken ct) =>
        HandleResult(await sender.Send(new ArchiveContentCommand(id), ct));

    /// <summary>
    /// Çeviri ekler.
    /// </summary>
    [HttpPost("{id:guid}/translations")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> AddTranslation(
        [FromRoute] Guid id,
        [FromBody] AddTranslationCommand command,
        CancellationToken ct) =>
        HandleResult(await sender.Send(command with { ContentId = id }, ct));

    /// <summary>
    /// Toplu işlem: Publish, Archive veya SoftDelete.
    /// Maksimum 100 item.
    /// BUG FIX (BulkActionHandler): Tenant sahiplik doğrulaması zaten handler'da mevcut.
    /// </summary>
    [HttpPost("bulk")]
    [Authorize]
    [ProducesResponseType(typeof(BulkActionResult), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Bulk(
        [FromBody] BulkActionCommand command,
        CancellationToken ct) =>
        HandleResult(await sender.Send(command, ct));

    /// <summary>
    /// Çöp kutusu listesi.
    /// </summary>
    [HttpGet("trash")]
    [Authorize]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Trash(CancellationToken ct) =>
        HandleResult(await sender.Send(new GetTrashQuery(), ct));
}


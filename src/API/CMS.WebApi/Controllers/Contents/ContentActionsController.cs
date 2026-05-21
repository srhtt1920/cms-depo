using CMS.Application.Features.Contents.DuplicateContent;
using CMS.Application.Features.Contents.HardDelete;
using CMS.Application.Features.Contents.Restore;
using CMS.Application.Features.Contents.UnpublishContent;
using CMS.WebApi.Controllers.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.WebApi.Controllers.Contents;

[Route("api/contents")]
[Authorize]
public sealed class ContentActionsController(ISender sender) : ApiController
{
    /// <summary>Yayından kaldır → Draft statüsüne döner.</summary>
    [HttpPost("{id:guid}/unpublish")]
    public async Task<IActionResult> Unpublish(
        [FromRoute] Guid id, CancellationToken ct) =>
        HandleResult(await sender.Send(new UnpublishContentCommand(id), ct));

    /// <summary>İçeriği kopyala. Yeni slug zorunlu.</summary>
    [HttpPost("{id:guid}/duplicate")]
    public async Task<IActionResult> Duplicate(
        [FromRoute] Guid id,
        [FromBody] DuplicateRequest body,
        CancellationToken ct) =>
        HandleResult(await sender.Send(new DuplicateContentCommand(id, body.NewSlug), ct));

    /// <summary>Kalıcı sil — sadece çöp kutusundaki içerikler.</summary>
    [HttpDelete("{id:guid}/hard")]
    public async Task<IActionResult> HardDelete(
        [FromRoute] Guid id, CancellationToken ct) =>
        HandleResult(await sender.Send(new HardDeleteContentCommand(id), ct));

    /// <summary>Çöp kutusundan geri al.</summary>
    [HttpPost("{id:guid}/restore")]
    public async Task<IActionResult> Restore(
        [FromRoute] Guid id, CancellationToken ct) =>
        HandleResult(await sender.Send(new RestoreContentCommand(id), ct));
}

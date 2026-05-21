using CMS.Application.Features.Contents.GetVersions;
using CMS.Application.Features.Contents.RestoreVersion;
using CMS.WebApi.Controllers.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.WebApi.Controllers.Contents;

[Route("api/contents/{contentId:guid}/versions")]
[Authorize]
public sealed class ContentVersionsController(ISender sender) : ApiController
{
    /// <summary>İçerik versiyon geçmişini listeler.</summary>
    [HttpGet]
    public async Task<IActionResult> GetVersions(
        [FromRoute] Guid contentId, CancellationToken ct) =>
        HandleResult(await sender.Send(new GetVersionsQuery(contentId), ct));

    /// <summary>Belirtilen versiyona geri döner.</summary>
    [HttpPost("{versionNumber:int}/restore")]
    public async Task<IActionResult> Restore(
        [FromRoute] Guid contentId,
        [FromRoute] int versionNumber,
        CancellationToken ct) =>
        HandleResult(await sender.Send(
            new RestoreVersionCommand(contentId, versionNumber), ct));
}

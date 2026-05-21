using CMS.Application.Features.Contents.Completion;
using CMS.Application.Features.Contents.HardDelete;
using CMS.Application.Features.Contents.Restore;
using CMS.WebApi.Controllers.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.WebApi.Controllers.Contents;

[Route("api/contents/trash")]
[Authorize]
public sealed class ContentTrashController(ISender sender) : ApiController
{
    [HttpGet("{id:guid}/completion")]
    public async Task<IActionResult> GetCompletion(Guid id, CancellationToken ct) =>
        HandleResult(await sender.Send(new GetCompletionQuery(id), ct));

    [HttpDelete("{id:guid}/hard")]
    public async Task<IActionResult> HardDelete(Guid id, CancellationToken ct) =>
        HandleResult(await sender.Send(new HardDeleteContentCommand(id), ct));

    [HttpPost("{id:guid}/restore")]
    public async Task<IActionResult> Restore(Guid id, CancellationToken ct) =>
        HandleResult(await sender.Send(new RestoreContentCommand(id), ct));
}

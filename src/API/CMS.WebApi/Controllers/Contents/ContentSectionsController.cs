using CMS.Application.Features.Contents.Blocks.DeleteBlock;
using CMS.Application.Features.Contents.Blocks.UpsertBlock;
using CMS.Application.Features.Contents.Blocks.UpsertBlockTranslation;
using CMS.Application.Features.Contents.GetContentWithSections;
using CMS.Application.Features.Contents.Sections.CreateSection;
using CMS.Application.Features.Contents.Sections.DeleteSection;
using CMS.Application.Features.Contents.Sections.ReorderSection;
using CMS.Application.Features.Contents.Sections.UpdateSection;
using CMS.WebApi.Controllers.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.WebApi.Controllers.Contents;

[Route("api/contents/{contentId:guid}")]
[Authorize]
public sealed class ContentSectionsController(ISender sender) : ApiController
{
    [HttpGet("sections")]
    public async Task<IActionResult> GetWithSections(
        [FromRoute] Guid contentId, CancellationToken ct) =>
        HandleResult(await sender.Send(new GetContentWithSectionsQuery(contentId), ct));

    [HttpPost("sections")]
    public async Task<IActionResult> CreateSection(
        [FromRoute] Guid contentId,
        [FromBody] CreateSectionCommand cmd,
        CancellationToken ct) =>
        HandleResult(await sender.Send(cmd with { ContentId = contentId }, ct));

    [HttpPut("sections/{sectionId:guid}")]
    public async Task<IActionResult> UpdateSection(
        [FromRoute] Guid contentId,
        [FromRoute] Guid sectionId,
        [FromBody] UpdateSectionCommand cmd,
        CancellationToken ct) =>
        HandleResult(await sender.Send(
            cmd with { ContentId = contentId, SectionId = sectionId }, ct));

    /// <summary>Section sıralamasını günceller. Body: [{sectionId, order}]</summary>
    [HttpPut("sections/reorder")]
    public async Task<IActionResult> ReorderSections(
        [FromRoute] Guid contentId,
        [FromBody] List<SectionOrderItem> items,
        CancellationToken ct) =>
        HandleResult(await sender.Send(
            new ReorderSectionsCommand(contentId, items), ct));

    [HttpDelete("sections/{sectionId:guid}")]
    public async Task<IActionResult> DeleteSection(
        [FromRoute] Guid contentId,
        [FromRoute] Guid sectionId,
        CancellationToken ct) =>
        HandleResult(await sender.Send(
            new DeleteSectionCommand(contentId, sectionId), ct));

    [HttpPost("sections/{sectionId:guid}/blocks")]
    public async Task<IActionResult> UpsertBlock(
        [FromRoute] Guid contentId,
        [FromRoute] Guid sectionId,
        [FromBody] UpsertBlockCommand cmd,
        CancellationToken ct) =>
        HandleResult(await sender.Send(
            cmd with { ContentId = contentId, SectionId = sectionId }, ct));

    [HttpPost("sections/{sectionId:guid}/blocks/{blockId:guid}/translations")]
    public async Task<IActionResult> UpsertBlockTranslation(
        [FromRoute] Guid contentId,
        [FromRoute] Guid sectionId,
        [FromRoute] Guid blockId,
        [FromBody] UpsertBlockTranslationCommand cmd,
        CancellationToken ct) =>
        HandleResult(await sender.Send(
            cmd with { ContentId = contentId, SectionId = sectionId, BlockId = blockId }, ct));

    [HttpDelete("sections/{sectionId:guid}/blocks/{blockId:guid}")]
    public async Task<IActionResult> DeleteBlock(
        [FromRoute] Guid contentId,
        [FromRoute] Guid sectionId,
        [FromRoute] Guid blockId,
        CancellationToken ct) =>
        HandleResult(await sender.Send(
            new DeleteBlockCommand(contentId, sectionId, blockId), ct));
}

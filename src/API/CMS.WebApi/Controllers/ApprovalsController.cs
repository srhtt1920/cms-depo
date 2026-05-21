using CMS.Application.Features.Approvals.ApproveContent;
using CMS.Application.Features.Approvals.GetApprovalDetail;
using CMS.Application.Features.Approvals.GetPendingApprovals;
using CMS.Application.Features.Approvals.RejectContent;
using CMS.WebApi.Controllers.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.WebApi.Controllers;

/// <summary>
/// İçerik onay merkezi API'si.
/// ApprovalCenter.razor tarafından kullanılır.
/// </summary>
[Route("api/approvals")]
[Authorize]
public sealed class ApprovalsController(ISender sender) : ApiController
{
    /// <summary>
    /// Onay bekleyen veya reddedilen içerikleri listeler.
    /// status parametresi: PendingApproval | Rejected | (boş = hepsi)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status = "PendingApproval",
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default) =>
        HandleResult(await sender.Send(
            new GetPendingApprovalsQuery(status, pageIndex, pageSize), ct));

    /// <summary>Onay detayını ve içerik çevirilerini döner.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct) =>
        HandleResult(await sender.Send(new GetApprovalDetailQuery(id), ct));

    /// <summary>İçeriği onaylar; opsiyonel yorumla.</summary>
    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Approve(
        Guid id,
        [FromBody] ApproveRequest req,
        CancellationToken ct) =>
        HandleResult(await sender.Send(new ApproveContentCommand(id, req.Comment), ct));

    /// <summary>İçeriği reddeder; zorunlu red sebebiyle.</summary>
    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Reject(
        Guid id,
        [FromBody] RejectRequest req,
        CancellationToken ct) =>
        HandleResult(await sender.Send(new RejectContentCommand(id, req.Reason), ct));
}

// ── Request DTO'lar ───────────────────────────────────────────────────────────
public sealed record ApproveRequest(string? Comment);
public sealed record RejectRequest(string Reason);


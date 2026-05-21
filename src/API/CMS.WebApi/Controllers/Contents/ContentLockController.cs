using CMS.Application.Features.Contents.Lock;
using CMS.Application.Features.Contents.Lock.BulkLockContents;
using CMS.Application.Features.Contents.Lock.GetContentLock;
using CMS.Application.Features.Contents.Lock.GetPageLock;
using CMS.Application.Features.Contents.Lock.SetContentLock;
using CMS.Application.Features.Contents.Lock.SetPageLock;
using CMS.WebApi.Controllers.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.WebApi.Controllers.Contents;

/// <summary>
/// İçerik kilit yönetimi.
/// ContentLockBadge.razor ve SuperAdminPanel.razor tarafından kullanılır.
/// </summary>
[Authorize]
public sealed class ContentLockController(ISender sender) : ApiController
{
    // ── Content Lock ─────────────────────────────────────────────────────────

    /// <summary>Belirtilen içeriğin kilit durumunu döner.</summary>
    [HttpGet("api/contents/{contentId:guid}/lock")]
    [ProducesResponseType(typeof(ContentLockDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetContentLock(Guid contentId, CancellationToken ct) =>
        HandleResult(await sender.Send(new GetContentLockQuery(contentId), ct));

    /// <summary>Belirtilen içeriği kilitler veya kilidini açar.</summary>
    [HttpPut("api/contents/{contentId:guid}/lock")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetContentLock(
        Guid contentId,
        [FromBody] SetLockRequest req,
        CancellationToken ct) =>
        HandleResult(await sender.Send(
            new SetContentLockCommand(contentId, req.IsLocked, req.LockType ?? "Soft", req.LockReason, req.RequiresSuperAdmin), ct));

    /// <summary>Birden fazla içeriği toplu olarak kilitler/kilidini açar.</summary>
    [HttpPost("api/contents/bulk-lock")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> BulkLock(
        [FromBody] BulkLockRequest req,
        CancellationToken ct) =>
        HandleResult(await sender.Send(
            new BulkLockContentsCommand(req.ContentIds, req.IsLocked, req.LockType ?? "Soft"), ct));

    // ── Page Lock ─────────────────────────────────────────────────────────────

    /// <summary>Belirtilen sayfanın kilit durumunu döner.</summary>
    [HttpGet("api/pages/{pageId:guid}/lock")]
    [ProducesResponseType(typeof(ContentLockDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPageLock(Guid pageId, CancellationToken ct) =>
        HandleResult(await sender.Send(new GetPageLockQuery(pageId), ct));

    /// <summary>Belirtilen sayfayı kilitler veya kilidini açar.</summary>
    [HttpPut("api/pages/{pageId:guid}/lock")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetPageLock(
        Guid pageId,
        [FromBody] SetLockRequest req,
        CancellationToken ct) =>
        HandleResult(await sender.Send(
            new SetPageLockCommand(pageId, req.IsLocked, req.LockType ?? "Soft"), ct));
}

// ── Request DTO'lar ───────────────────────────────────────────────────────────

/// <summary>İçerik kilit ayarlama isteği.</summary>
public sealed record SetLockRequest(bool IsLocked, string? LockType, string? LockReason = null, bool RequiresSuperAdmin = false);

/// <summary>Toplu kilit isteği.</summary>
public sealed record BulkLockRequest(List<Guid> ContentIds, bool IsLocked, string? LockType);
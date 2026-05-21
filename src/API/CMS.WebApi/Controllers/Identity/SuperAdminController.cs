using CMS.Application.Features.SuperAdmin;
using CMS.Application.Features.SuperAdmin.AssignUserToTenant;
using CMS.Application.Features.SuperAdmin.GetSuperAdminAudit;
using CMS.Application.Features.SuperAdmin.GetSuperAdmins;
using CMS.Application.Features.SuperAdmin.GetTenantDetail;
using CMS.Application.Features.SuperAdmin.GetTenantMaintenance;
using CMS.Application.Features.SuperAdmin.GetTransferPreview;
using CMS.Application.Features.SuperAdmin.GrantSuperAdmin;
using CMS.Application.Features.SuperAdmin.RemoveUserFromTenant;
using CMS.Application.Features.SuperAdmin.RevokeSuperAdmin;
using CMS.Application.Features.SuperAdmin.SetMaintenanceMode;
using CMS.Application.Features.SuperAdmin.SetTenantActive;
using CMS.Application.Features.SuperAdmin.TransferUser;
using CMS.WebApi.Controllers.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.WebApi.Controllers.Identity;

// <summary>
/// SuperAdmin işlemleri — yalnızca SuperAdmin rolüne sahip kullanıcılar erişebilir.
/// Tüm tenant'lar üzerinde yetki gerektirir.
/// </summary>
[Route("api/superadmin")]
[Authorize(Policy = "SuperAdminOnly")]   // Program.cs'de policy tanımlanmalı
public sealed class SuperAdminController(ISender sender) : ApiController
{
    // ── Admin Yönetimi ────────────────────────────────────────────────────────

    /// <summary>Tüm SuperAdmin kullanıcılarını listeler.</summary>
    [HttpGet("admins")]
    [ProducesResponseType(typeof(List<SuperAdminListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAdmins(CancellationToken ct) =>
        HandleResult(await sender.Send(new GetSuperAdminsQuery(), ct));

    /// <summary>Kullanıcıya SuperAdmin yetkisi verir.</summary>
    [HttpPost("grant")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Grant(
        [FromBody] GrantSuperAdminRequest req,
        CancellationToken ct) =>
        HandleResult(await sender.Send(new GrantSuperAdminCommand(req.UserId), ct));

    /// <summary>Kullanıcıdan SuperAdmin yetkisini kaldırır.</summary>
    [HttpDelete("revoke")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Revoke(
        [FromBody] RevokeSuperAdminRequest req,
        CancellationToken ct) =>
        HandleResult(await sender.Send(new RevokeSuperAdminCommand(req.UserId), ct));

    /// <summary>SuperAdmin audit loglarını sayfalı olarak döner.</summary>
    [HttpGet("audit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAudit(
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default) =>
        HandleResult(await sender.Send(new GetSuperAdminAuditQuery(pageIndex, pageSize), ct));

    // ── Tenant Yönetimi ───────────────────────────────────────────────────────

    /// <summary>Tenant detayını getirir (kullanıcı sayısı, diller, bakım modu dahil).</summary>
    [HttpGet("tenants/{tenantId:guid}")]
    [ProducesResponseType(typeof(TenantDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTenantDetail(Guid tenantId, CancellationToken ct) =>
        HandleResult(await sender.Send(new GetTenantDetailQuery(tenantId), ct));

    /// <summary>Tenant'ı aktif veya pasif yapar.</summary>
    [HttpPut("tenants/{tenantId:guid}/active")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetTenantActive(
        Guid tenantId,
        [FromBody] SetTenantActiveRequest req,
        CancellationToken ct) =>
        HandleResult(await sender.Send(new SetTenantActiveCommand(tenantId, req.IsActive), ct));

    /// <summary>Tenant'ın bakım modu durumunu döner.</summary>
    [HttpGet("tenants/{tenantId:guid}/maintenance")]
    [ProducesResponseType(typeof(TenantMaintenanceDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMaintenance(Guid tenantId, CancellationToken ct) =>
        HandleResult(await sender.Send(new GetTenantMaintenanceQuery(tenantId), ct));

    /// <summary>Tenant'ın bakım modunu açar veya kapatır.</summary>
    [HttpPut("tenants/{tenantId:guid}/maintenance")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetMaintenance(
        Guid tenantId,
        [FromBody] SetMaintenanceModeRequest req,
        CancellationToken ct) =>
        HandleResult(await sender.Send(new SetMaintenanceModeCommand(tenantId, req.IsMaintenanceMode), ct));

    // ── Kullanıcı Transfer ────────────────────────────────────────────────────

    /// <summary>Kullanıcıyı farklı tenant'a transfer etmeden önce önizleme döner.</summary>
    [HttpGet("users/{userId:guid}/transfer-preview")]
    [ProducesResponseType(typeof(UserTransferPreviewDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTransferPreview(
        Guid userId,
        [FromQuery] Guid sourceTenantId,
        CancellationToken ct) =>
        HandleResult(await sender.Send(new GetTransferPreviewQuery(userId, sourceTenantId), ct));

    /// <summary>Kullanıcıyı kaynak tenant'tan hedef tenant'a taşır.</summary>
    [HttpPost("users/transfer")]
    [ProducesResponseType(typeof(TransferUserResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> TransferUser(
        [FromBody] TransferUserRequest req,
        CancellationToken ct) =>
        HandleResult(await sender.Send(
            new TransferUserCommand(req.UserId, req.SourceTenantId, req.TargetTenantId, req.KeepSource), ct));

    // ── Tenant Kullanıcı Yönetimi ─────────────────────────────────────────────

    /// <summary>Kullanıcıyı belirtilen tenant'a atar ve rolleri düzenler.</summary>
    [HttpPost("tenants/{tenantId:guid}/assign-user")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AssignUser(
        Guid tenantId,
        [FromBody] AssignUserToTenantRequest req,
        CancellationToken ct) =>
        HandleResult(await sender.Send(
            new AssignUserToTenantCommand(req.UserId, tenantId, req.RoleIds), ct));

    /// <summary>Kullanıcıyı belirtilen tenant'tan kaldırır (tüm rolleri iptal edilir).</summary>
    [HttpDelete("tenants/{tenantId:guid}/users/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveUser(
        Guid tenantId,
        Guid userId,
        CancellationToken ct) =>
        HandleResult(await sender.Send(new RemoveUserFromTenantCommand(tenantId, userId), ct));
}

// ── Request DTO'lar ───────────────────────────────────────────────────────────

public sealed record GrantSuperAdminRequest(Guid UserId, string? ConfirmPhrase = null);
public sealed record RevokeSuperAdminRequest(Guid UserId, string? Reason = null);
public sealed record SetTenantActiveRequest(bool IsActive, string? Reason = null, string? Message = null);
public sealed record SetMaintenanceModeRequest(bool IsMaintenanceMode, string? Title = null, string? Message = null, string? AllowedEmailPattern = null, DateTime? PlannedEndAt = null);
public sealed record TransferUserRequest(Guid UserId, Guid SourceTenantId, Guid TargetTenantId, List<Guid>? RoleIds = null, bool KeepSource = false, bool TransferContent = false);
public sealed record AssignUserToTenantRequest(Guid UserId, List<Guid> RoleIds);
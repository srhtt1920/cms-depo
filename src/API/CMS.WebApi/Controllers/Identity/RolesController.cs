using CMS.Application.Features.Identity.AssignRole;
using CMS.Application.Features.Identity.CreateRole;
using CMS.Application.Features.Identity.DeleteRole;
using CMS.Application.Features.Identity.GetRoleById;
using CMS.Application.Features.Identity.GetRoles;
using CMS.Application.Features.Identity.RevokeRole;
using CMS.Application.Features.Identity.UpdateRole;
using CMS.WebApi.Controllers.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.WebApi.Controllers.Identity;

[Authorize]
public sealed class RolesController(ISender sender) : ApiController
{
    /// <summary>Aktif tenant'taki rol listesi.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<RoleListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await sender.Send(new GetRolesQuery(), ct);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RoleDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
    [FromRoute] Guid id,
    CancellationToken ct) =>
    HandleResult(await sender.Send(new GetRoleByIdQuery(id), ct));


    /// <summary>
    /// Yeni rol oluşturur.
    /// TenantId sunucu tarafında JWT'den alınır.
    /// PermissionKeys: "content.read" gibi string key'ler.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateRoleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(
        [FromBody] CreateRoleRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new CreateRoleCommand(request.Name, request.PermissionKeys), ct);
        if (result.IsFailure) return HandleResult(result);
        return CreatedAtAction(nameof(GetAll),
            new { id = result.Value.RoleId }, result.Value);
    }

    /// <summary>Rol adı ve permission listesini günceller.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(RoleListDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateRoleRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new UpdateRoleCommand(id, request.Name, request.PermissionKeys), ct);
        return HandleResult(result);
    }

    /// <summary>Rolü pasife alır — atandığı kullanıcılardan önce kaldırılır.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var result = await sender.Send(new DeleteRoleCommand(id), ct);
        return HandleResult(result);
    }

    /// <summary>Kullanıcıya rol atar.</summary>
    [HttpPost("{roleId:guid}/assign")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Assign(
        [FromRoute] Guid roleId,
        [FromBody] AssignRoleRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new AssignRoleCommand(request.UserId, request.TenantId, roleId), ct);
        return HandleResult(result);
    }

    /// <summary>Kullanıcıdan rol kaldırır.</summary>
    [HttpDelete("{roleId:guid}/revoke")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Revoke(
        [FromRoute] Guid roleId,
        [FromBody] RevokeRoleRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new RevokeRoleCommand(request.UserId, request.TenantId, roleId), ct);
        return HandleResult(result);
    }
}

// Request DTO'ları — Controller-level
public sealed record CreateRoleRequest(string Name, List<string> PermissionKeys);
public sealed record UpdateRoleRequest(string Name, List<string> PermissionKeys);
public sealed record AssignRoleRequest(Guid UserId, Guid TenantId);
public sealed record RevokeRoleRequest(Guid UserId, Guid TenantId);

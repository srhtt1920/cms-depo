using CMS.Application.Common.Abstractions;
using CMS.Application.Features.Identity.GetPermissions;
using CMS.WebApi.Controllers.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.WebApi.Controllers.Identity;

[Route("api/me")]
[Authorize]
public sealed class PermissionsController(
    ISender sender,
    ICurrentUser currentUser) : ApiController
{
    /// <summary>
    /// Kullanıcının aktif tenant'taki permission tree'sini döner.
    /// </summary>
    [HttpGet("permissions")]
    [ProducesResponseType(typeof(PermissionTreeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPermissions(CancellationToken ct)
    {
        // TenantId doğrudan JWT'den — TenantResolutionBehavior'a bağımlı değil
        var tenantId = currentUser.CurrentTenantId;

        if (tenantId == Guid.Empty)
            return BadRequest(new { error = "Active tenant not found in token. Call /auth/select-tenant first." });

        var result = await sender.Send(
            new GetPermissionsQuery(currentUser.UserId, tenantId), ct);

        return HandleResult(result);
    }
}

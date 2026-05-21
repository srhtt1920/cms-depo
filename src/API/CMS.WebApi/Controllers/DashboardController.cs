using CMS.Application.Features.Dashboard.GetDashboardStats;
using CMS.WebApi.Controllers.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.WebApi.Controllers;

[Route("api/dashboard")]
[Authorize]
public sealed class DashboardController(ISender sender) : ApiController
{
    /// <summary>
    /// Aktif tenant'a ait dashboard istatistiklerini döner.
    /// İçerik sayıları, kullanıcı sayıları, son aktiviteler.
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(DashboardStatsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats(CancellationToken ct) =>
        HandleResult(await sender.Send(new GetDashboardStatsQuery(), ct));
}
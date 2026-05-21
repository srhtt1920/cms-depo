using CMS.Application.Features.AuditLogs;
using CMS.WebApi.Controllers.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.WebApi.Controllers;

[Route("api/audit-logs")]
[Authorize]
public sealed class AuditLogsController(ISender sender) : ApiController
{
    /// <summary>Tenant audit logu — kim ne yaptı.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] GetAuditLogsQuery query, CancellationToken ct) =>
        HandleResult(await sender.Send(query, ct));
}

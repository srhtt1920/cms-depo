using CMS.Application.Features.Contents.Schedule;
using CMS.Application.Features.Contents.ScheduleCalendar;
using CMS.WebApi.Controllers.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.WebApi.Controllers.Contents;

[Route("api/contents/{contentId:guid}/schedule")]
[Authorize]
public sealed class ContentScheduleController(ISender sender) : ApiController
{
    [HttpPost]
    public async Task<IActionResult> Set(
        Guid contentId, [FromBody] SetScheduleCommand cmd, CancellationToken ct) =>
        HandleResult(await sender.Send(cmd with { ContentId = contentId }, ct));

    [HttpDelete]
    public async Task<IActionResult> Clear(Guid contentId, CancellationToken ct) =>
        HandleResult(await sender.Send(new ClearScheduleCommand(contentId), ct));

    [HttpGet("/api/schedule/calendar")]
    [Authorize]
    [ProducesResponseType(typeof(List<ScheduleCalendarItemDto>), 200)]
    public async Task<IActionResult> GetCalendar(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            CancellationToken ct) =>
            HandleResult(await sender.Send(new GetScheduleCalendarQuery(from, to), ct));

}

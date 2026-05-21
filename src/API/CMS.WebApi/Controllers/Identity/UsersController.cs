using CMS.Application.Features.Identity.CreateUser;
using CMS.Application.Features.Identity.DeleteUser;
using CMS.Application.Features.Identity.GetUserActivity;
using CMS.Application.Features.Identity.GetUserById;
using CMS.Application.Features.Identity.GetUsers;
using CMS.Application.Features.Identity.SetUserActive;
using CMS.Application.Features.Identity.UpdateUserRoles;
using CMS.WebApi.Controllers.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.WebApi.Controllers.Identity;

[Route("api/users")]
[Authorize]
public sealed class UsersController(ISender sender) : ApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        HandleResult(await sender.Send(new GetUsersQuery(), ct));

    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid userId, CancellationToken ct) =>
        HandleResult(await sender.Send(new GetUserByIdQuery(userId), ct));

    [HttpGet("{userId:guid}/activity")]
    [ProducesResponseType(typeof(UserActivityPageDto), 200)]
    public async Task<IActionResult> GetActivity(
        Guid userId,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 30,
        CancellationToken ct = default) =>
        HandleResult(await sender.Send(new GetUserActivityQuery(userId, pageIndex, pageSize), ct));

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateUserCommand command, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        if (result.IsFailure) return HandleResult(result);
        return CreatedAtAction(nameof(GetById), new { userId = result.Value.Id }, result.Value);
    }

    [HttpPut("{userId:guid}/roles")]
    public async Task<IActionResult> UpdateRoles(
        [FromRoute] Guid userId,
        [FromBody] UpdateUserRolesCommand command,
        CancellationToken ct) =>
        HandleResult(await sender.Send(command with { UserId = userId }, ct));

    [HttpPut("{userId:guid}/active")]
    public async Task<IActionResult> SetActive(
        [FromRoute] Guid userId,
        [FromBody] SetUserActiveCommand command,
        CancellationToken ct) =>
        HandleResult(await sender.Send(command with { UserId = userId }, ct));

    [HttpDelete("{userId:guid}")]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid userId, CancellationToken ct) =>
        HandleResult(await sender.Send(new DeleteUserCommand(userId), ct));
}

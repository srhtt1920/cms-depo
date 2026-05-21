using CMS.Application.Features.Tenants.CreateTenant;
using CMS.Application.Features.Tenants.DeactivateTenant;
using CMS.Application.Features.Tenants.GetTenants;
using CMS.Application.Features.Tenants.GetTenantSettings;
using CMS.Application.Features.Tenants.UpdateLanguages;
using CMS.Application.Features.Tenants.UpdateTenant;
using CMS.WebApi.Controllers.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.WebApi.Controllers.Tenants;

[Route("api/tenants")]
[Authorize]
public sealed class TenantsController(ISender sender) : ApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        HandleResult(await sender.Send(new GetTenantsQuery(), ct));

    [HttpGet("{id:guid}/settings")]
    public async Task<IActionResult> GetSettings(
        [FromRoute] Guid id, CancellationToken ct) =>
        HandleResult(await sender.Send(new GetTenantSettingsQuery(id), ct));

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateTenantCommand command, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        if (result.IsFailure) return HandleResult(result);
        return CreatedAtAction(nameof(GetSettings), new { id = result.Value.TenantId }, result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateTenantRequest body,
        CancellationToken ct) =>
        HandleResult(await sender.Send(new UpdateTenantCommand(id, body.Name), ct));

    [HttpPut("{id:guid}/languages")]
    public async Task<IActionResult> UpdateLanguages(
        [FromRoute] Guid id,
        [FromBody] UpdateLanguagesCommand command,
        CancellationToken ct) =>
        HandleResult(await sender.Send(command with { TenantId = id }, ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(
        [FromRoute] Guid id, CancellationToken ct) =>
        HandleResult(await sender.Send(new DeactivateTenantCommand(id), ct));
}



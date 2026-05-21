using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Tenants.DeactivateTenant;

public sealed record DeactivateTenantCommand(Guid TenantId) : IRequest<Result>;

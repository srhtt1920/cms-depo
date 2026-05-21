using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Tenants.UpdateTenant;

public sealed record UpdateTenantCommand(Guid TenantId, string Name) : IRequest<Result>;

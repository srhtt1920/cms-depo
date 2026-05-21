using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Tenants.GetTenantSettings;

public sealed record GetTenantSettingsQuery(Guid TenantId)
    : IRequest<Result<TenantSettingsDto>>;

using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Tenants.GetTenants;

public sealed record GetTenantsQuery : IRequest<Result<List<TenantListDto>>>;

public sealed record TenantListDto(
    Guid         Id,
    string       Name,
    List<string> Languages,
    string       DefaultLanguage,
    bool         IsActive);

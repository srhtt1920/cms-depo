using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Tenants.CreateTenant;

public sealed record CreateTenantCommand(
    string Name,
    string DefaultLanguageCode = "tr") : IRequest<Result<CreateTenantResponse>>;

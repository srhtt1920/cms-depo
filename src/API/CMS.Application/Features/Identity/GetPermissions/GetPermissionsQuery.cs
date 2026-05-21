using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Identity.GetPermissions;

public sealed record GetPermissionsQuery(Guid UserId, Guid TenantId)
    : IRequest<Result<PermissionTreeDto>>;

using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Identity.RevokeRole;

public sealed record RevokeRoleCommand(
    Guid UserId,
    Guid TenantId,
    Guid RoleId) : IRequest<Result>;

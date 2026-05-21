using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Identity.AssignRole;

public sealed record AssignRoleCommand(
    Guid UserId,
    Guid TenantId,
    Guid RoleId) : IRequest<Result>;

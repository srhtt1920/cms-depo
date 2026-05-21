using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Identity.DeleteRole;

public sealed record DeleteRoleCommand(Guid RoleId) : IRequest<Result>;

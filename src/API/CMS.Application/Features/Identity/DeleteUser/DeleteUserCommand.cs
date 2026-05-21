using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Identity.DeleteUser;

public sealed record DeleteUserCommand(Guid UserId) : IRequest<Result>;

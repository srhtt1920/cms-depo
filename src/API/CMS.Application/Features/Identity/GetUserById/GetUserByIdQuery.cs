
using CMS.Application.Features.Identity.GetUsers;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Identity.GetUserById;

public sealed record GetUserByIdQuery(Guid UserId) : IRequest<Result<UserListDto>>;

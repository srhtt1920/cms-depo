using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Identity.SetUserActive;

public sealed record SetUserActiveCommand(Guid UserId, bool IsActive)
    : IRequest<Result>;

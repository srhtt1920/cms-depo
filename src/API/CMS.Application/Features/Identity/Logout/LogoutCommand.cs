using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Identity.Logout;

public sealed record LogoutCommand(string? RefreshToken = null) : IRequest<Result>;

using CMS.Application.Features.Identity.Login;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Identity.RefreshToken;
public sealed record RefreshTokenCommand(string Token) : IRequest<Result<LoginResponse>>;



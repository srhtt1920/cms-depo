using CMS.Application.Common.Abstractions;
using CMS.Domain.Common;
using CMS.Domain.Identity;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Identity.Logout;

public sealed class LogoutHandler(
ICurrentUser currentUser,
ITokenBlacklist blacklist,
IRefreshTokenRepository refreshTokens,
IUserRepository userRepository)
: IRequestHandler<LogoutCommand, Result>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken ct)
    {
        // Access token'ı blacklist'e ekle (jti claim'inden)
        // JTI HttpContext'ten CurrentUser üzerinden gelmeli — şimdilik userId bazlı
        // Gerçek implementasyonda JWT middleware'den jti claim okunur

        // Refresh token revoke
        if (!string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            var rt = await refreshTokens.GetByTokenAsync(request.RefreshToken, ct);
            if (rt is not null && rt.IsActive)
            {
                rt.Revoke();
                await refreshTokens.UpdateAsync(rt, ct);
            }
        }

        return Result.Success();
    }
}

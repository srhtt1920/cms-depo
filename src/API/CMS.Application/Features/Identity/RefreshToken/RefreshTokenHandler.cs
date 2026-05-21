using CMS.Application.Common.Abstractions;
using CMS.Application.Features.Identity.Login;
using CMS.Domain.Common;
using CMS.Domain.Identity;
using CMS.Domain.Tenants;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Identity.RefreshToken;

public sealed class RefreshTokenHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IUserRepository userRepository,
    ITenantRepository tenantRepository,
    IJwtService jwtService)
    : IRequestHandler<RefreshTokenCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(
        RefreshTokenCommand request, CancellationToken ct)
    {
        var storedToken = await refreshTokenRepository.GetByTokenAsync(request.Token, ct);

        if (storedToken is null || !storedToken.IsActive)
            return Result.Failure<LoginResponse>(
                Error.Unauthorized("Auth.InvalidRefreshToken", "Geçersiz veya süresi dolmuş token."));

        var user = await userRepository.GetWithRolesAsync(storedToken.UserId, ct);
        if (user is null || !user.IsActive)
            return Result.Failure<LoginResponse>(
                Error.Unauthorized("Auth.UserNotFound", "Kullanıcı bulunamadı."));

        // Token rotation — eski token'ı revoke et, yeni üret
        var newRefreshToken = Domain.Identity.RefreshToken.Create(user.Id);
        storedToken.Revoke(newRefreshToken.Token);

        await refreshTokenRepository.UpdateAsync(storedToken, ct);
        await refreshTokenRepository.AddAsync(newRefreshToken, ct);

        var tenantIds = user.GetTenantIds().ToList();
        var tenants = new List<TenantInfo>();
        foreach (var tid in tenantIds)
        {
            var tenant = await tenantRepository.GetByIdAsync(tid, ct);
            if (tenant is not null)
                tenants.Add(new TenantInfo(tid.Value, tenant.Name));
        }

        var accessToken = jwtService.GenerateToken(user);
        return new LoginResponse(
            accessToken,
            jwtService.GetExpiry(),
            user.Email,
            user.DisplayName,
            tenants,
            false,
            newRefreshToken.Token,
            Guid.Empty
            );
    }
}
using CMS.Application.Common.Abstractions;
using CMS.Application.Features.Identity.Login;
using CMS.Domain.Identity;
using CMS.Domain.Tenants;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Identity.TwoFactor.Verify;

public sealed record TwoFactorVerifyCommand(string Code, string Email) : IRequest<Result<TwoFactorVerifyResponse>>;

public sealed record TwoFactorVerifyResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    string Email,
    string? DisplayName,
    List<TenantInfo> Tenants
);

public sealed class TwoFactorVerifyHandler(
    IUserRepository userRepo,
    ITotpService totpService,
    ITenantRepository tenantRepository,
    IJwtService jwtService)
    : IRequestHandler<TwoFactorVerifyCommand, Result<TwoFactorVerifyResponse>>
{
    public async Task<Result<TwoFactorVerifyResponse>> Handle(
        TwoFactorVerifyCommand req, CancellationToken ct)
    {
        var user = await userRepo.GetByEmailAsync(req.Email.ToLowerInvariant().Trim(), ct);
        if (user is null || !user.IsTwoFactorEnabled || user.TwoFactorSecret is null)
            return Result.Failure<TwoFactorVerifyResponse>(
                Error.Validation("2FA.NotEnabled", "2FA bu hesap için aktif değil."));

        // TOTP doğrulaması
        var isValid = totpService.ValidateCode(user.TwoFactorSecret, req.Code)
                   || user.UseBackupCode(req.Code); // backup code deneme

        if (!isValid)
            return Result.Failure<TwoFactorVerifyResponse>(
                Error.Validation("2FA.InvalidCode", "Geçersiz veya süresi dolmuş kod."));

        await userRepo.UpdateAsync(user, ct); // backup code kullanıldıysa güncelle

        var userWithRoles = await userRepo.GetWithRolesAsync(user.Id, ct);

        var tenantIds = userWithRoles?.GetTenantIds().ToList();
        var tenants = new List<TenantInfo>();
        foreach (var tid in tenantIds)
        {
            var tenant = await tenantRepository.GetByIdAsync(tid, ct);
            if (tenant is not null)
                tenants.Add(new TenantInfo(tid.Value, tenant.Name));
        }


        var (access, refresh) = await jwtService.GenerateTokenPairAsync(user, ct);
        var expiresAt = jwtService.GetExpiry();

        return new TwoFactorVerifyResponse(access, refresh, expiresAt,user.Email,user.DisplayName,tenants);
    }
}

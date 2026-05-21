using CMS.Application.Common.Abstractions;
using CMS.Domain.Identity;
using CMS.Domain.Tenants;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Identity.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<Result<LoginResponse>>;

public record LoginResponse(
    string Token,
    DateTime ExpiresAt,
    string Email,
    string? DisplayName,
    List<TenantInfo> Tenants,
    bool RequiresTwoFactor = false,
    string? RefreshToken = null,
    Guid? AutoSelectedTenantId = null);

public record TenantInfo(Guid TenantId, string Name, bool IsSystem = false);

public sealed class LoginHandler(
        IUserRepository userRepository,
        ITenantRepository tenantRepository,
        IJwtService jwtService,
        IPasswordHasher passwordHasher) : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private static readonly Error InvalidCredentials =
        Error.Unauthorized("Auth.InvalidCredentials", "Geçersiz e-posta veya şifre.");

    public async Task<Result<LoginResponse>> Handle(
        LoginCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, ct);
        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            return Result.Failure<LoginResponse>(InvalidCredentials);

        if (!user.IsActive)
            return Result.Failure<LoginResponse>(
                Error.Forbidden("Auth.UserInactive", "Hesabınız devre dışı."));

        var userWithRoles = await userRepository.GetWithRolesAsync(user.Id, ct);
        if (userWithRoles is null)
            return Result.Failure<LoginResponse>(InvalidCredentials);

        userWithRoles.RecordLogin();
        await userRepository.UpdateAsync(userWithRoles, ct);

        // Tenant listesini oluştur
        var tenantIds = userWithRoles.GetTenantIds().ToList();
        var tenants = new List<TenantInfo>();

        foreach (var tid in tenantIds)
        {
            var tenant = await tenantRepository.GetByIdAsync(tid, ct);
            if (tenant is null) continue;

            // ── SYSTYEM TENANT SELECTOR'DAN GİZLENİR ──────────────────────
            // System tenant yalnızca IsSuperAdmin kullanıcılara gösterilmez;
            // direkt JWT'ye bağlanır. Business tenant selector'a eklenir.
            if (tenant.IsSystem)
            {
                // SuperAdmin ise System'a direkt giriş seçeneği oluştur
                if (user.IsSuperAdmin)
                    tenants.Add(new TenantInfo(
                        tid.Value, tenant.Name, IsSystem: true));
                // Normal kullanıcılar System'u görmez
                continue;
            }

            tenants.Add(new TenantInfo(tid.Value, tenant.Name, IsSystem: false));
        }

        var token = jwtService.GenerateToken(userWithRoles);
        var expiresAt = jwtService.GetExpiry();

        if (user.IsTwoFactorEnabled)
            return new LoginResponse(token, expiresAt, userWithRoles.Email,
                user.DisplayName, tenants, RequiresTwoFactor: true);

        // ── AUTO-SELECT LOGIC ───────────────────────────────────────────
        // Sadece 1 business tenant varsa → otomatik seç, direkt token dön
        var businessTenants = tenants.Where(t => !t.IsSystem).ToList();

        if (businessTenants.Count == 1 && !user.IsSuperAdmin)
        {
            var autoTenant = businessTenants[0];
            var dbTenant = await tenantRepository.GetByIdAsync(
                TenantId.From(autoTenant.TenantId), ct);

            var autoToken = jwtService.GenerateToken(
                userWithRoles, autoTenant.TenantId, autoTenant.Name,
                TenantType.Business);

            return new LoginResponse(autoToken, expiresAt,
                userWithRoles.Email, user.DisplayName, tenants,
                AutoSelectedTenantId: autoTenant.TenantId);
        }

        // Sadece System varsa (SuperAdmin, tek tenant) → direkt Root'a gir
        if (tenants.Count == 1 && tenants[0].IsSystem && user.IsSuperAdmin)
        {
            var systemTenant = await tenantRepository.GetByIdAsync(
                TenantId.From(tenants[0].TenantId), ct);

            var systemToken = jwtService.GenerateToken(
                userWithRoles, tenants[0].TenantId, tenants[0].Name,
                TenantType.System);

            return new LoginResponse(systemToken, expiresAt,
                userWithRoles.Email, user.DisplayName, tenants,
                AutoSelectedTenantId: tenants[0].TenantId);
        }

        // Birden fazla tenant → selector'a yönlendir (token tenant seçilmemiş)
        return new LoginResponse(token, expiresAt,
            userWithRoles.Email, user.DisplayName, tenants);
    }
}
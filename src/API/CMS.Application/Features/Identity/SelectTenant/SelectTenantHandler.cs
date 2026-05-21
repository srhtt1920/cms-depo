using CMS.Application.Common.Abstractions;
using CMS.Application.Features.Identity.Login;
using CMS.Domain.Identity;
using CMS.Domain.Tenants;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Identity.SelectTenant;
public sealed record SelectTenantCommand(Guid TenantId) : IRequest<Result<SelectTenantResponse>>;
public sealed record SelectTenantResponse(
    string Token,
    DateTime ExpiresAt,
    Guid TenantId,
    string Name, 
    TenantType TenantType);

public sealed class SelectTenantHandler(
    ICurrentUser currentUser,
    IUserRepository userRepository,
    ITenantRepository tenantRepository,
    IJwtService jwtService) : IRequestHandler<SelectTenantCommand, Result<SelectTenantResponse>>
{
    public async Task<Result<SelectTenantResponse>> Handle(
        SelectTenantCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetWithRolesAsync(
            UserId.From(currentUser.UserId), ct);

        if (user is null)
            return Result.Failure<SelectTenantResponse>(
                Error.NotFound("User.NotFound", "Kullanıcı bulunamadı."));

        var tenant = await tenantRepository.GetByIdAsync(
            TenantId.From(request.TenantId), ct);

        if (tenant is null)
            return Result.Failure<SelectTenantResponse>(
                Error.NotFound("Tenant.NotFound", "Tenant bulunamadı."));

        // Root tenant'a geçiş: sadece SuperAdmin yapabilir
        if (tenant.IsSystem && !user.IsSuperAdmin)
            return Result.Failure<SelectTenantResponse>(
                Error.Forbidden("Tenant.System.AccessDenied",
                    "System tenant'a erişim için SuperAdmin yetkisi gereklidir."));

        // Normal tenant erişim kontrolü
        if (!tenant.IsSystem)
        {
            var hasTenantAccess = user.TenantRoles
                .Any(r => r.TenantId == TenantId.From(request.TenantId));

            if (!hasTenantAccess)
                return Result.Failure<SelectTenantResponse>(
                    Error.Forbidden("Tenant.AccessDenied",
                        "Bu tenant'a erişim yetkiniz yok."));
        }

        var token = jwtService.GenerateToken(
            user, request.TenantId, tenant.Name, tenant.Type);  // ← TenantType eklendi

        return new SelectTenantResponse(
            token,
            jwtService.GetExpiry(),
            request.TenantId,
            tenant.Name,
            tenant.Type);  // ← response'a da eklendi
    }
}

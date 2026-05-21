using CMS.Domain.Identity;
using CMS.Domain.Tenants;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.SuperAdmin.GetTenantDetail;

public sealed record GetTenantDetailQuery(Guid TenantId) : IRequest<Result<TenantDetailDto>>;

[RequirePermission("superadmin")]
public sealed class GetTenantDetailHandler(
    ITenantRepository tenantRepo,
    IUserRepository userRepo)
    : IRequestHandler<GetTenantDetailQuery, Result<TenantDetailDto>>
{
    public async Task<Result<TenantDetailDto>> Handle(GetTenantDetailQuery req, CancellationToken ct)
    {
        var tenantId = TenantId.From(req.TenantId);
        var tenant = await tenantRepo.GetByIdAsync(tenantId, ct);
        if (tenant is null)
            return Result.Failure<TenantDetailDto>(Error.NotFound("Tenant.NotFound", "Tenant not found."));

        var users = await userRepo.GetByTenantAsync(tenantId, ct);
        var isMaint = tenant.IsMaintenanceMode; // Domain'e eklenmeli — aşağıya bakın

        return new TenantDetailDto(
            tenant.Id.Value, tenant.Name,
            tenant.DefaultLanguageCode,
            tenant.IsActive,
            isMaint,
            users.Count,
            tenant.CreatedAt);
    }
}

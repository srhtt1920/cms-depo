using CMS.Domain.Identity;
using CMS.Domain.Tenants;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.SuperAdmin.GetTransferPreview;

public sealed record GetTransferPreviewQuery(Guid UserId, Guid SourceTenantId)
    : IRequest<Result<UserTransferPreviewDto>>;

[RequirePermission("superadmin")]
public sealed class GetTransferPreviewHandler(
    IUserRepository userRepo,
    ITenantRepository tenantRepo,
    IRoleRepository roleRepo)
    : IRequestHandler<GetTransferPreviewQuery, Result<UserTransferPreviewDto>>
{
    public async Task<Result<UserTransferPreviewDto>> Handle(
        GetTransferPreviewQuery req, CancellationToken ct)
    {
        var user = await userRepo.GetWithRolesAsync(UserId.From(req.UserId), ct);
        if (user is null)
            return Result.Failure<UserTransferPreviewDto>(Error.NotFound("User.NotFound", "User not found."));

        var sourceTenant = await tenantRepo.GetByIdAsync(TenantId.From(req.SourceTenantId), ct);
        if (sourceTenant is null)
            return Result.Failure<UserTransferPreviewDto>(Error.NotFound("Tenant.NotFound", "Source tenant not found."));

        var tenantId = TenantId.From(req.SourceTenantId);
        var roleIds = user.TenantRoles
            .Where(r => r.TenantId == tenantId)
            .Select(r => r.RoleId)
            .ToList();
        var roles = await roleRepo.GetByTenantAsync(tenantId, ct);
        var roleNames = roles
            .Where(r => roleIds.Contains(r.Id))
            .Select(r => r.Name)
            .ToList();

        return new UserTransferPreviewDto(
            user.Id.Value, user.Email, user.DisplayName,
            req.SourceTenantId, sourceTenant.Name, roleNames);
    }
}
using CMS.Application.Common.Abstractions;
using CMS.Domain.Identity;
using CMS.Domain.Tenants;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Identity.GetRoleById;

public sealed record GetRoleByIdQuery(Guid RoleId) : IRequest<Result<RoleDetailDto>>;

public sealed record RoleDetailDto(
    Guid Id,
    string Name,
    List<string> PermissionKeys,
    int UserCount,
    bool IsActive,
    DateTime CreatedAt);

[RequirePermission("roles.manage")]
public sealed class GetRoleByIdHandler(
    IRoleRepository roleRepository,
    IUserRepository userRepository,
    ITenantContext tenantContext)
    : IRequestHandler<GetRoleByIdQuery, Result<RoleDetailDto>>
{
    public async Task<Result<RoleDetailDto>> Handle(GetRoleByIdQuery req, CancellationToken ct)
    {
        var tenantId = TenantId.From(tenantContext.TenantId);
        var role = await roleRepository.GetWithPermissionsAsync(RoleId.From(req.RoleId), ct);

        if (role is null || role.TenantId != tenantId)
            return Result.Failure<RoleDetailDto>(
                Error.NotFound("Role.NotFound", $"Role '{req.RoleId}' not found."));

        // Bu role'e atanmış kullanıcı sayısı
        var users = await userRepository.GetByTenantAsync(tenantId, ct);
        var userCount = users.Count(u =>
            u.TenantRoles.Any(r => r.TenantId == tenantId && r.RoleId == role.Id));

        return new RoleDetailDto(
            role.Id.Value,
            role.Name,
            role.Permissions.Select(p => p.Key).ToList(),
            userCount,
            role.IsActive,
            role.CreatedAt);
    }
}

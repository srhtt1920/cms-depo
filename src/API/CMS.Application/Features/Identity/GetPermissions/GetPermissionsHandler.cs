using CMS.Application.Common.Abstractions;
using CMS.Domain.Identity;
using CMS.Domain.Tenants;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Identity.GetPermissions;

/// <summary>
/// Her çağrıda:
/// 1. JWT'deki permV ile cache'deki permV karşılaştırılır (IPermissionService içinde).
/// 2. Değişmişse permission'lar DB'den yenilenir.
/// 3. PermissionTree build edilir ve döner.
/// </summary>
public sealed class GetPermissionsHandler(
    IUserRepository userRepository,
    IPermissionRepository permissionRepository)
    : IRequestHandler<GetPermissionsQuery, Result<PermissionTreeDto>>
{
    public async Task<Result<PermissionTreeDto>> Handle(
        GetPermissionsQuery request, CancellationToken ct)
    {
        if (request.TenantId == Guid.Empty)
            return Result.Failure<PermissionTreeDto>(
                Error.Validation("Tenant.Required",
                    "TenantId boş. Önce /auth/select-tenant çağrısı yapılmalı."));

        var user = await userRepository.GetByIdAsync(
            UserId.From(request.UserId), ct);

        if (user is null)
            return Result.Failure<PermissionTreeDto>(
                Error.NotFound("User.NotFound", "Kullanıcı bulunamadı."));

        // Tüm sistem permission'larını çek (tree build için)
        var allPermissions = await permissionRepository.GetAllAsync(ct);

        // Bu user + tenant için granted key'leri çek
        // IPermissionService içinde version kontrolü + cache refresh yapılıyor
        var grantedKeys = await permissionRepository.GetKeysByUserAndTenantAsync(
            UserId.From(request.UserId),
            TenantId.From(request.TenantId),
            ct);

        // Tree build
        var tree = PermissionTree.Build(allPermissions, grantedKeys);

        return new PermissionTreeDto(
            user.PermissionVersion,
            [.. tree.Select(MapNode)]);
    }

    private static PermissionNodeDto MapNode(PermissionNode node) =>
        new(node.Key,
            node.DisplayName,
            node.Granted,
            [.. node.Children.Select(MapNode)]);
}

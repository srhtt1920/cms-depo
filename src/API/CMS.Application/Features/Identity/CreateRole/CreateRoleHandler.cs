using CMS.Application.Common.Abstractions;
using CMS.Domain.Identity;
using CMS.Domain.Tenants;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Identity.CreateRole;

[RequirePermission("roles.manage")]
public sealed class CreateRoleHandler(
    IRoleRepository roleRepository,
    IPermissionRepository permissionRepository,
    ITenantContext tenantContext)
    : IRequestHandler<CreateRoleCommand, Result<CreateRoleResponse>>
{
    public async Task<Result<CreateRoleResponse>> Handle(
        CreateRoleCommand request, CancellationToken ct)
    {
        var tenantId = TenantId.From(tenantContext.TenantId);
        var role = Role.Create(tenantId, request.Name);

        if (request.PermissionKeys.Count > 0)
        {
            var allPermissions = await permissionRepository.GetAllAsync(ct);
            foreach (var perm in allPermissions.Where(p =>
                request.PermissionKeys.Contains(p.Key,
                    StringComparer.OrdinalIgnoreCase)))
                role.AddPermission(perm);
        }

        await roleRepository.AddAsync(role, ct);

        return new CreateRoleResponse(role.Id.Value, role.Name);
    }
}
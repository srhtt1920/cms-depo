using CMS.Application.Common.Abstractions;
using CMS.Domain.Identity;
using CMS.Domain.Tenants;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Identity.ListRoles;

[RequirePermission("roles.manage")]
public sealed class ListRolesHandler(
    IRoleRepository roleRepository,
    IUserRepository userRepository,
    ITenantContext tenantContext)
    : IRequestHandler<ListRolesQuery, Result<ListRolesResponse>>
{
    public async Task<Result<ListRolesResponse>> Handle(ListRolesQuery request, CancellationToken ct)
    {
        var tenantId = TenantId.From(tenantContext.TenantId);

        var rolesQuery = await roleRepository.GetByTenantAsync(tenantId, ct);
        var activeRoles = rolesQuery.Where(r => r.IsActive).ToList();

        var users = await userRepository.GetByTenantAsync(tenantId, ct);

        var items = activeRoles.Select(r =>
        {
            var perms = r.Permissions.Select(p => p.Key).ToList();
            var userCount = users.Count(u =>
                u.TenantRoles.Any(utr => utr.TenantId == tenantId && utr.RoleId == r.Id));
            return new RoleListItem(r.Id.Value, r.Name, perms, userCount);
        }).ToList();

        // Paginate in-memory (repository-level pagination eklenene kadar)
        var paged = items
            .Skip(request.PageIndex * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var paginate = new SharedKernel.Pagination.Paginate<RoleListItem>
        {
            Index = request.PageIndex,
            Size  = request.PageSize,
            Count = items.Count,
            Items = paged
        };

        return new ListRolesResponse(paginate);
    }
}

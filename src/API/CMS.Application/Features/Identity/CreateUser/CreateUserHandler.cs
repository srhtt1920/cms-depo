using CMS.Application.Common.Abstractions;
using CMS.Domain.Identity;
using CMS.Domain.Tenants;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Identity.CreateUser;

public sealed record CreateUserCommand(
    string Email,
    string Password,
    List<Guid> RoleIds) : IRequest<Result<CreateUserResponse>>;

public sealed record CreateUserResponse(
    Guid Id,
    string Email,
    List<string> Roles,
    bool IsActive);

[RequirePermission("users.manage")]
public sealed class CreateUserHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IPasswordHasher hasher,
    ITenantContext tenantContext,
    IUserTenantAssignmentPolicy assignmentPolicy)  // ← YENİ
    : IRequestHandler<CreateUserCommand, Result<CreateUserResponse>>
{
    public async Task<Result<CreateUserResponse>> Handle(
        CreateUserCommand request, CancellationToken ct)
    {
        var existing = await userRepository.GetByEmailAsync(request.Email, ct);
        if (existing is not null)
            return Result.Failure<CreateUserResponse>(
                Error.Conflict("User.EmailExists",
                    $"'{request.Email}' already registered."));

        var tenantId = TenantId.From(tenantContext.TenantId);
        var tenantRoles = await roleRepository.GetByTenantAsync(tenantId, ct);
        var roleMap = tenantRoles.ToDictionary(r => r.Id.Value);
        var roleNames = new List<string>();

        foreach (var roleId in request.RoleIds)
        {
            if (!roleMap.TryGetValue(roleId, out var role)) continue;

            // ── SYSTEM TENANT ROLE KISITI ─────────────────────────────────
            var canAssign = await assignmentPolicy.CanAssignRoleAsync(
                tenantContext.TenantId, role.Name, ct);

            if (!canAssign)
                return Result.Failure<CreateUserResponse>(
                    Error.Forbidden("User.SystemRoleRestriction",
                        $"'{role.Name}' rolü System tenant'a atanamaz. " +
                        $"İzin verilen roller: " +
                        string.Join(", ", assignmentPolicy.GetAllowedRolesForSystemTenant())));

            roleNames.Add(role.Name);
        }

        var passwordHash = hasher.Hash(request.Password);
        var user = User.Create(request.Email, passwordHash);

        foreach (var roleId in request.RoleIds)
        {
            if (roleMap.TryGetValue(roleId, out var role))
                user.AssignRole(tenantId, role.Id);
        }

        await userRepository.AddAsync(user, ct);
        return new CreateUserResponse(user.Id.Value, user.Email, roleNames, true);
    }
}
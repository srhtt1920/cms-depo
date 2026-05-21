using CMS.Application.Common.Abstractions;
using CMS.Domain.Identity;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.SuperAdmin.GrantSuperAdmin;

public sealed record GrantSuperAdminCommand(Guid TargetUserId) : IRequest<Result>;

[RequirePermission("superadmin")]
public sealed class GrantSuperAdminHandler(
    IUserRepository userRepo,
    IAuditLogger auditLogger)
    : IRequestHandler<GrantSuperAdminCommand, Result>
{
    public async Task<Result> Handle(GrantSuperAdminCommand req, CancellationToken ct)
    {
        var user = await userRepo.GetByIdAsync(UserId.From(req.TargetUserId), ct);
        if (user is null)
            return Result.Failure(Error.NotFound("User.NotFound", "User not found."));

        user.GrantSuperAdmin();
        await userRepo.UpdateAsync(user, ct);
        await auditLogger.LogAsync("SuperAdmin.Grant", "User", req.TargetUserId.ToString(),
            $"SuperAdmin granted to {user.Email}", ct);
        return Result.Success();
    }
}

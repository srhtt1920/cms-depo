using CMS.Application.Common.Abstractions;
using CMS.Domain.Identity;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.SuperAdmin.RevokeSuperAdmin;

public sealed record RevokeSuperAdminCommand(Guid TargetUserId) : IRequest<Result>;

[RequirePermission("superadmin")]
public sealed class RevokeSuperAdminHandler(
    IUserRepository userRepo,
    IAuditLogger auditLogger)
    : IRequestHandler<RevokeSuperAdminCommand, Result>
{
    public async Task<Result> Handle(RevokeSuperAdminCommand req, CancellationToken ct)
    {
        var user = await userRepo.GetByIdAsync(UserId.From(req.TargetUserId), ct);
        if (user is null)
            return Result.Failure(Error.NotFound("User.NotFound", "User not found."));

        user.RevokeSuperAdmin();
        await userRepo.UpdateAsync(user, ct);
        await auditLogger.LogAsync("SuperAdmin.Revoke", "User", req.TargetUserId.ToString(),
            $"SuperAdmin revoked from {user.Email}", ct);
        return Result.Success();
    }
}

using CMS.Application.Common.Abstractions;
using CMS.Domain.Identity;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Identity.ResetPassword;

public sealed record ResetPasswordCommand(
    string Token,
    string Email,
    string NewPassword)
    : IRequest<Result>;

public sealed class ResetPasswordHandler(
    IUserRepository userRepo,
    IPasswordHasher hasher)
    : IRequestHandler<ResetPasswordCommand, Result>
{
    public async Task<Result> Handle(ResetPasswordCommand req, CancellationToken ct)
    {
        var user = await userRepo.GetByEmailAsync(req.Email.ToLowerInvariant().Trim(), ct);
        if (user is null)
            return Result.Failure(Error.NotFound("User.NotFound", "Invalid reset request."));

        if (!user.IsPasswordResetTokenValid(req.Token))
            return Result.Failure(Error.Validation("Token.Invalid",
                "Şifre sıfırlama bağlantısı geçersiz veya süresi dolmuş."));

        var newHash = hasher.Hash(req.NewPassword);
        user.ChangePassword(newHash);
        user.ClearPasswordResetToken();
        await userRepo.UpdateAsync(user, ct);
        return Result.Success();
    }
}

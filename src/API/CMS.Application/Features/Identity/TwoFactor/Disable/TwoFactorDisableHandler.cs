using CMS.Application.Common.Abstractions;
using CMS.Domain.Identity;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Identity.TwoFactor.Disable;

public sealed record TwoFactorDisableCommand(string CurrentPassword) : IRequest<Result>;

public sealed class TwoFactorDisableHandler(
    IUserRepository userRepo,
    ICurrentUser currentUser,
    IPasswordHasher hasher)
    : IRequestHandler<TwoFactorDisableCommand, Result>
{
    public async Task<Result> Handle(TwoFactorDisableCommand req, CancellationToken ct)
    {
        var user = await userRepo.GetByIdAsync(UserId.From(currentUser.UserId), ct);
        if (user is null)
            return Result.Failure(Error.NotFound("User.NotFound", "User not found."));

        if (!hasher.Verify(req.CurrentPassword, user.PasswordHash))
            return Result.Failure(Error.Validation("Password.Wrong", "Mevcut şifre hatalı."));

        user.Disable2Fa();
        await userRepo.UpdateAsync(user, ct);
        return Result.Success();
    }
}

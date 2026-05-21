using CMS.Application.Common.Abstractions;
using CMS.Domain.Identity;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Identity.ChangePassword;

public sealed class ChangePasswordHandler(
 IUserRepository userRepository,
 ICurrentUser currentUser,
 IPasswordHasher passwordHasher)
 : IRequestHandler<ChangePasswordCommand, Result>
{
    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken ct)
    {
        if (request.NewPassword != request.ConfirmPassword)
            return Result.Failure(Error.Validation("Password.Mismatch",
                "Yeni şifreler eşleşmiyor."));

        var user = await userRepository.GetByIdAsync(
            UserId.From(currentUser.UserId), ct);

        if (user is null)
            return Result.Failure(Error.NotFound("User.NotFound", "Kullanıcı bulunamadı."));

        if (!passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
            return Result.Failure(Error.Unauthorized("Auth.WrongPassword",
                "Mevcut şifre yanlış."));

        if (request.NewPassword.Length < 8)
            return Result.Failure(Error.Validation("Password.TooShort",
                "Şifre en az 8 karakter olmalı."));

        var newHash = passwordHasher.Hash(request.NewPassword);
        user.ChangePassword(newHash);
        await userRepository.UpdateAsync(user, ct);
        return Result.Success();
    }
}

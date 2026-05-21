using CMS.Application.Common.Abstractions;
using CMS.Domain.Identity;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Identity.TwoFactor.Resend;

public sealed record TwoFactorResendCommand(string Email) : IRequest<Result>;

public sealed class TwoFactorResendHandler(
    IUserRepository userRepo,
    IEmailSender emailSender)
    : IRequestHandler<TwoFactorResendCommand, Result>
{
    public async Task<Result> Handle(TwoFactorResendCommand req, CancellationToken ct)
    {
        var user = await userRepo.GetByEmailAsync(req.Email.ToLowerInvariant(), ct);
        if (user is null || !user.IsTwoFactorEnabled)
            return Result.Success(); // güvenlik için başarılı dön

        // TOTP tabanlı ise kod zaten authenticator'dan alınır;
        // e-posta bazlı OTP gerekiyorsa burada üretilip gönderilmeli
        // Kriptografik olarak guvenli rastgele OTP uretimi
        var otpBytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(4);
        var otpNum = (BitConverter.ToUInt32(otpBytes, 0) % 900000) + 100000;
        var otp = otpNum.ToString();
        user.SetEmailOtp(otp, DateTime.UtcNow.AddMinutes(10));
        await userRepo.UpdateAsync(user, ct);
        await emailSender.SendAsync(user.Email, "Doğrulama Kodu",
            $"<p>Giriş doğrulama kodunuz: <strong>{otp}</strong></p><p>10 dakika geçerlidir.</p>", ct);
        return Result.Success();
    }
}
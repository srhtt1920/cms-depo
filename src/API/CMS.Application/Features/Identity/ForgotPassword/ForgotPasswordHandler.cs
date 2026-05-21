using CMS.Application.Common.Abstractions;
using CMS.Domain.Identity;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Identity.ForgotPassword;

public sealed record ForgotPasswordCommand(string Email) : IRequest<Result>;

public sealed class ForgotPasswordHandler(
    IUserRepository userRepo,
    IEmailSender emailSender)
    : IRequestHandler<ForgotPasswordCommand, Result>
{
    private const int TokenValidMinutes = 60;

    public async Task<Result> Handle(ForgotPasswordCommand req, CancellationToken ct)
    {
        var user = await userRepo.GetByEmailAsync(req.Email.ToLowerInvariant().Trim(), ct);

        // Güvenlik: kullanıcı bulunsun ya da bulunmasın başarılı dön
        // (e-posta numaralandırma saldırısını önlemek için)
        if (user is null) return Result.Success();

        // Token üret ve kaydet
        var token = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));
        var expiresAt = DateTime.UtcNow.AddMinutes(TokenValidMinutes);
        user.SetPasswordResetToken(token, expiresAt);
        await userRepo.UpdateAsync(user, ct);

        // Reset linki — frontend URL'i environment'tan alınmalı
        var resetLink = $"https://yourdomain.com/reset-password?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(user.Email)}";
        var body = $"""
            <p>Şifrenizi sıfırlamak için aşağıdaki bağlantıya tıklayın:</p>
            <p><a href="{resetLink}">Şifremi Sıfırla</a></p>
            <p>Bu bağlantı <strong>{TokenValidMinutes} dakika</strong> geçerlidir.</p>
            <p>Bu isteği siz yapmadıysanız bu e-postayı dikkate almayın.</p>
            """;

        await emailSender.SendAsync(user.Email, "Şifre Sıfırlama", body, ct);
        return Result.Success();
    }
}

using CMS.Application.Common.Abstractions;
using CMS.Domain.Identity;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Identity.TwoFactor.Setup;

public sealed record TwoFactorSetupCommand : IRequest<Result<TwoFactorSetupResponse>>;

public sealed record TwoFactorSetupResponse(
    string QrCodeUri,     // otpauth:// URI — QR olarak render edilir
    string ManualKey,     // elle girilecek secret key
    string[] BackupCodes);

public sealed class TwoFactorSetupHandler(
    IUserRepository userRepo,
    ICurrentUser currentUser,
    ITotpService totpService)
    : IRequestHandler<TwoFactorSetupCommand, Result<TwoFactorSetupResponse>>
{
    public async Task<Result<TwoFactorSetupResponse>> Handle(
        TwoFactorSetupCommand _, CancellationToken ct)
    {
        var user = await userRepo.GetByIdAsync(UserId.From(currentUser.UserId), ct);
        if (user is null)
            return Result.Failure<TwoFactorSetupResponse>(Error.NotFound("User.NotFound", "User not found."));

        var secret = totpService.GenerateSecret();
        var qrUri = totpService.GetQrCodeUri(user.Email, secret, "CMS Panel");
        var backupCodes = totpService.GenerateBackupCodes(8);

        user.Setup2Fa(secret, backupCodes);
        await userRepo.UpdateAsync(user, ct);

        return new TwoFactorSetupResponse(qrUri, secret, backupCodes);
    }
}

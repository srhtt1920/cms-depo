namespace CMS.Application.Common.Abstractions;
public interface ITotpService
{
    string GenerateSecret();
    string GetQrCodeUri(string email, string secret, string issuer);
    string[] GenerateBackupCodes(int count);
    bool ValidateCode(string secret, string code);
}
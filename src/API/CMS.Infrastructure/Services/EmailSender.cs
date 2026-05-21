using CMS.Application.Common.Abstractions;
using Microsoft.Extensions.Logging;

namespace CMS.Infrastructure.Services;

/// <summary>
/// E-posta gönderici — şu an loglama tabanlı stub implementasyon.
/// Üretim ortamında SendGrid, SMTP veya benzeri bir servis entegre edilmelidir.
/// </summary>
public class EmailSender(ILogger<EmailSender> logger) : IEmailSender
{
    public Task SendAsync(
        string to,
        string subject,
        string htmlBody,
        CancellationToken ct = default)
    {
        // TODO: Üretim ortamında gerçek e-posta sağlayıcısı entegre edilecek
        // Örn: SendGrid, MailKit/SMTP, Azure Communication Services
        logger.LogInformation(
            "E-posta gönderim isteği — To: {To} | Subject: {Subject}",
            to, subject);

        return Task.CompletedTask;
    }
}

namespace CMS.Application.Common.Abstractions;

/// <summary>
/// HTML içeriği XSS saldırılarına karşı temizler.
/// Uygulama: Ganss.Xss (HtmlSanitizer NuGet paketi).
/// </summary>
public interface ICmsHtmlSanitizer
{
    /// <summary>Güvenli HTML döner. Tehlikeli tag ve attribute'lar kaldırılır.</summary>
    string Sanitize(string html);
}

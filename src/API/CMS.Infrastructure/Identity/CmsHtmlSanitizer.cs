using CMS.Application.Common.Abstractions;
using Ganss.Xss;


namespace CMS.Infrastructure.Identity;

/// <summary>
/// Ganss.Xss (HtmlSanitizer) tabanlı whitelist sanitization.
/// İzin verilen tag'ler: p, h1-h6, ul, ol, li, a, strong, em, blockquote, code, pre, table, tr, td, th, img
/// </summary>
public sealed class CmsHtmlSanitizer : ICmsHtmlSanitizer
{
    private readonly HtmlSanitizer _sanitizer;

    public CmsHtmlSanitizer()
    {
        _sanitizer = new HtmlSanitizer();

        // Ekstra izin verilen tag'ler
        _sanitizer.AllowedTags.Add("h1");
        _sanitizer.AllowedTags.Add("h2");
        _sanitizer.AllowedTags.Add("h3");
        _sanitizer.AllowedTags.Add("h4");
        _sanitizer.AllowedTags.Add("h5");
        _sanitizer.AllowedTags.Add("h6");
        _sanitizer.AllowedTags.Add("blockquote");
        _sanitizer.AllowedTags.Add("code");
        _sanitizer.AllowedTags.Add("pre");
        _sanitizer.AllowedTags.Add("figure");
        _sanitizer.AllowedTags.Add("figcaption");

        // İzin verilen attribute'lar
        _sanitizer.AllowedAttributes.Add("class");
        _sanitizer.AllowedAttributes.Add("id");
        _sanitizer.AllowedAttributes.Add("alt");
        _sanitizer.AllowedAttributes.Add("title");

        // Tehlikeli attribute'ları çıkar
        _sanitizer.AllowedAttributes.Remove("style");
    }

    public string Sanitize(string html)
    {
        if (string.IsNullOrWhiteSpace(html)) return string.Empty;
        return _sanitizer.Sanitize(html);
    }
}

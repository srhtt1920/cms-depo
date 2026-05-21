using CMS.Application.Features.Contents.Seo;
using CMS.WebApi.Controllers.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Xml.Linq;

namespace CMS.WebApi.Controllers;

[Route("api/seo")]
public sealed class SeoController(ISender sender) : ApiController
{
    /// <summary>Tenant sitemap.xml — CDN'e cache'lenmesi önerilir.</summary>
    [HttpGet("sitemap.xml")]
    [ResponseCache(Duration = 3600)]
    public async Task<IActionResult> Sitemap(CancellationToken ct)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var result  = await sender.Send(new GetSitemapQuery(baseUrl), ct);
        if (result.IsFailure) return NotFound();

        var ns  = XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9");
        var xsi = XNamespace.Get("http://www.w3.org/1999/xhtml");
        var urlset = new XElement(ns + "urlset",
            new XAttribute("xmlns", ns),
            new XAttribute(XNamespace.Xmlns + "xhtml", xsi));

        foreach (var entry in result.Value.Entries)
        {
            var url = new XElement(ns + "url",
                new XElement(ns + "loc",        entry.Loc),
                new XElement(ns + "lastmod",    entry.LastMod),
                new XElement(ns + "changefreq", entry.ChangeFreq),
                new XElement(ns + "priority",   entry.Priority.ToString("F1")));

            foreach (var alt in entry.Alternates)
                url.Add(new XElement(xsi + "link",
                    new XAttribute("rel",      "alternate"),
                    new XAttribute("hreflang", alt.HrefLang),
                    new XAttribute("href",     alt.Href)));

            urlset.Add(url);
        }

        var xml = new XDocument(new XDeclaration("1.0", "utf-8", null), urlset);
        return Content(xml.ToString(), "application/xml", Encoding.UTF8);
    }

    /// <summary>İçerik meta tag + JSON-LD verisi.</summary>
    [HttpGet("contents/{slug}/meta")]
    [ResponseCache(Duration = 300)]
    public async Task<IActionResult> ContentMeta(
        string slug, [FromQuery] string lang = "tr", CancellationToken ct = default) =>
        HandleResult(await sender.Send(new GetContentMetaQuery(slug, lang), ct));
}

using CMS.Application.Common.Abstractions;
using CMS.Domain.Contents;
using CMS.Domain.Tenants;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.Seo;

[RequirePermission("content.read")]
public sealed class GetContentMetaHandler(
    IContentRepository repo,
    ITenantRepository  tenantRepo,
    ITenantContext     tenantCtx)
    : IRequestHandler<GetContentMetaQuery, Result<ContentMetaDto>>
{
    public async Task<Result<ContentMetaDto>> Handle(
        GetContentMetaQuery req, CancellationToken ct)
    {
        var content = await repo.GetBySlugAsync(req.Slug, tenantCtx.TenantId, ct);
        if (content is null || content.Status != ContentStatus.Published)
            return Result.Failure<ContentMetaDto>(
                Error.NotFound("Content.NotFound", $"'{req.Slug}' not found or not published."));

        var tenant = await tenantRepo.GetByIdAsync(TenantId.From(tenantCtx.TenantId), ct);

        var translation = content.Translations.FirstOrDefault(t => t.LanguageCode == req.Lang)
            ?? content.Translations.FirstOrDefault(t =>
                t.LanguageCode == (tenant?.DefaultLanguageCode ?? "en"));

        if (translation is null)
            return Result.Failure<ContentMetaDto>(
                Error.NotFound("Content.NoTranslation", $"No translation for '{req.Lang}'."));

        var langs     = tenant?.SupportedLanguages.Select(l => l.LanguageCode).ToList() ?? [req.Lang];
        var baseUrl   = ""; // Runtime'da host header'dan alınır; handler'a inject edilebilir
        var alternates = langs
            .Where(l => content.Translations.Any(t => t.LanguageCode == l))
            .Select(l => new HreflangEntry(l, $"{baseUrl}/{l}/{content.Slug.Value}"))
            .ToList();

        return new ContentMetaDto(
            Title:              translation.MetaTitle ?? translation.Title,
            Description:        translation.MetaDescription ?? "",
            OgTitle:            translation.MetaTitle ?? translation.Title,
            OgDescription:      translation.MetaDescription,
            OgImageUrl:         null, // SeoMeta.OgImageUrl eklenince buraya
            CanonicalUrl:       $"/{req.Lang}/{content.Slug.Value}",
            Robots:             "index, follow",
            StructuredDataJson: BuildArticleJsonLd(content, translation),
            HreflangAlternates: alternates);
    }

    private static string BuildArticleJsonLd(Content content, ContentTranslation t) =>
        $$"""
        {
          "@context": "https://schema.org",
          "@type": "Article",
          "headline": "{{t.Title.Replace("\"", "\\\"")}}", 
          "datePublished": "{{content.CreatedAt:yyyy-MM-dd}}",
          "dateModified": "{{(content.UpdatedAt ?? content.CreatedAt):yyyy-MM-dd}}"
        }
        """;
}

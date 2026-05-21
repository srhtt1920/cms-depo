using CMS.Application.Common.Abstractions;
using CMS.Domain.Contents;
using CMS.Domain.Tenants;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.Seo;

[RequirePermission("content.read")]
public sealed class GetSitemapHandler(
    IContentRepository repo,
    ITenantRepository  tenantRepo,
    ITenantContext     tenantCtx)
    : IRequestHandler<GetSitemapQuery, Result<SitemapDto>>
{
    public async Task<Result<SitemapDto>> Handle(GetSitemapQuery req, CancellationToken ct)
    {
        var tenantId = TenantId.From(tenantCtx.TenantId);
        var tenant   = await tenantRepo.GetByIdAsync(tenantId, ct);
        if (tenant is null)
            return Result.Failure<SitemapDto>(Error.NotFound("Tenant.NotFound", "Tenant not found."));

        var published = await repo.GetPublishedAsync(tenantId, ct);
        var langs     = tenant.SupportedLanguages.Select(l => l.LanguageCode).ToList();

        var entries = published.Select(content =>
        {
            var lastMod  = (content.UpdatedAt ?? content.CreatedAt).ToString("yyyy-MM-dd");
            var baseUrl  = req.BaseUrl.TrimEnd('/');

            // Hreflang alternates — tüm dillerde çevirisi olan içerikler için
            var alternates = langs
                .Where(lang => content.Translations.Any(t => t.LanguageCode == lang))
                .Select(lang => new HreflangEntry(
                    lang == tenant.DefaultLanguageCode ? "x-default" : lang,
                    $"{baseUrl}/{lang}/{content.Slug.Value}"))
                .ToList();

            return new SitemapEntry(
                Loc:        $"{baseUrl}/{content.Slug.Value}",
                LastMod:    lastMod,
                ChangeFreq: "weekly",
                Priority:   0.8,
                Alternates: alternates);
        }).ToList();

        return new SitemapDto(entries);
    }
}

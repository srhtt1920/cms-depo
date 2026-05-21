using CMS.Application.Common.Abstractions;
using CMS.Domain.Contents;
using CMS.Domain.Tenants;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.Completion;

[RequirePermission("content.detailRead")]
public sealed class GetCompletionHandler(
    IContentRepository contentRepository,
    ITenantRepository tenantRepository,
    ITenantContext tenantCtx)
    : IRequestHandler<GetCompletionQuery, Result<ContentCompletionDto>>
{
    // BlockType enum → zorunlu çeviri alanları
    private static readonly Dictionary<BlockType, string[]> RequiredFields = new()
    {
        [BlockType.Text] = ["Title", "Body"],
        [BlockType.Image] = ["AltText"],
        [BlockType.Slider] = ["Title"],
        [BlockType.Card] = ["Title", "Body"],
        [BlockType.Link] = ["LinkText"],
        [BlockType.Action] = ["LinkText"],
        [BlockType.Custom] = [],
    };

    public async Task<Result<ContentCompletionDto>> Handle(
            GetCompletionQuery request, CancellationToken ct)
    {
        var content = await contentRepository
            .GetByIdWithSectionsAsync(ContentId.From(request.ContentId), ct);

        if (content is null)
            return Result.Failure<ContentCompletionDto>(
                Error.NotFound("Content.NotFound", "Content not found."));

        var tenant = await tenantRepository.GetByIdAsync(
            TenantId.From(tenantCtx.TenantId), ct);

        if (tenant is null)
            return Result.Failure<ContentCompletionDto>(
                Error.NotFound("Tenant.NotFound", "Tenant not found."));

        var requiredLangs = tenant.SupportedLanguages
            .Select(l => l.LanguageCode)
            .ToList();

        // Tüm blokları düz liste olarak topla (tüm section'lardan)
        var allBlocks = content.Sections
            .OrderBy(s => s.Order)
            .SelectMany(s => s.Blocks)
            .ToList();

        // Section bazlı skor
        var sectionScores = content.Sections
            .OrderBy(s => s.Order)
            .Select(s => new SectionScore(
                s.Id.Value,
                s.Name,
                s.Blocks.OrderBy(b => b.Order).Select(b =>
                {
                    var missing = requiredLangs
                        .Where(lang =>
                        {
                            var fields = GetRequired(b.BlockType);
                            if (fields.Length == 0) return false;
                            var trans = b.Translations
                                .FirstOrDefault(t => t.LanguageCode == lang);
                            return fields.Any(f => !HasValue(trans, f));
                        }).ToList();

                    int score = requiredLangs.Count == 0 ? 100
                        : (int)(((double)(requiredLangs.Count - missing.Count)
                                 / requiredLangs.Count) * 100);

                    return new BlockScore(
                        b.Id.Value, b.BlockType.ToString(), score, missing);
                }).ToList()))
            .ToList();

        // Dil bazlı skor
        var langScores = requiredLangs.Select(lang =>
        {
            var missing = new List<string>();

            var blockScoresForLang = allBlocks.Select(b =>
            {
                var fields = GetRequired(b.BlockType);
                if (fields.Length == 0) return 100;

                var trans = b.Translations
                    .FirstOrDefault(t => t.LanguageCode == lang);

                var missingF = fields.Where(f => !HasValue(trans, f)).ToList();
                if (missingF.Count > 0)
                    missing.Add($"block:{b.Id.Value}:{string.Join(",", missingF)}");

                return (int)(((double)(fields.Length - missingF.Count)
                              / fields.Length) * 100);
            }).ToList();

            int score = blockScoresForLang.Count == 0 ? 100
                : (int)blockScoresForLang.Average();

            return new LangScore(lang, score, missing);
        }).ToList();

        int overall = langScores.Count == 0 ? 0
            : (int)langScores.Average(l => l.Score);

        var canPublishLangs = langScores
            .Where(l => l.Score == 100)
            .Select(l => l.Code)
            .ToList();

        return new ContentCompletionDto(
            content.Id.Value,
            content.Slug.Value,
            overall,
            requiredLangs,
            langScores,
            sectionScores,
            canPublishLangs.Count > 0 && overall >= 100,
            canPublishLangs);
    }

    private static List<ContentBlock> GetAllBlocks(
        IReadOnlyList<ContentSection> sections)
    {
        var result = new List<ContentBlock>();
        foreach (var s in sections)
        {
            result.AddRange(s.Blocks);
            // Alt section'lar (ParentSectionId != null) aynı listede flat geliyor;
            // tree yapısı repository tarafından tek sorguda yükleniyor.
        }
        return result;
    }

    private static SectionScore ScoreSection(ContentSection section, List<string> langs)
    {
        var blockScores = section.Blocks.Select(b =>
        {
            var missingLangs = langs.Where(lang =>
            {
                var required = BlockRequiredFields.For(b.BlockType);
                if (required.Count == 0) return false;
                var trans = b.Translations.FirstOrDefault(t => t.LanguageCode == lang);
                return required.Any(f => !HasValue(trans, f));
            }).ToList();

            int score = langs.Count == 0 ? 100
                : (int)(((double)(langs.Count - missingLangs.Count) / langs.Count) * 100);
            return new BlockScore(b.Id.Value, b.BlockType.ToString(), score, missingLangs);
        }).ToList();

        return new SectionScore(section.Id.Value, section.Name, blockScores);
    }

    private static string[] GetRequired(BlockType t) =>
        RequiredFields.TryGetValue(t, out var f) ? f : [];

    private static bool HasValue(BlockTranslation? t, string field) => field switch
    {
        "Title" => t is not null && !string.IsNullOrWhiteSpace(t.Title),
        "Body" => t is not null && !string.IsNullOrWhiteSpace(t.Body),
        "AltText" => t is not null && !string.IsNullOrWhiteSpace(t.AltText),
        "LinkText" => t is not null && !string.IsNullOrWhiteSpace(t.LinkText),
        _ => true
    };
}

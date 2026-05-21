namespace CMS.Application.Features.Contents.Completion;

public sealed record ContentCompletionDto(
    Guid                        ContentId,
    string                      Slug,
    int                         OverallScore,
    IReadOnlyList<string>       RequiredLanguages,
    IReadOnlyList<LangScore>    Languages,
    IReadOnlyList<SectionScore> Sections,
    bool                        CanPublish,
    IReadOnlyList<string>       CanPublishLanguages
);

public sealed record LangScore(string Code, int Score, IReadOnlyList<string> MissingFields);
public sealed record SectionScore(Guid SectionId, string Name, IReadOnlyList<BlockScore> Blocks);
public sealed record BlockScore(Guid BlockId, string Type, int Score, IReadOnlyList<string> MissingTranslations);

namespace CMS.Blazor.Server.Panel.Models.Content;

// ── List ───────────────────────────────────────────────────────────────────
public sealed record ListContentsResponse(ContentPagedDto Data);

public sealed record ContentPagedDto(
    int Index, int Size, int Count, int Pages,
    bool HasPrevious, bool HasNext,
    List<ContentSummaryDto> Items);

public sealed record ContentSummaryDto(
    Guid Id,
    string Slug,
    string Status,
    string ContentType,
    string Title,
    List<string> Languages,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

// ── Detail ─────────────────────────────────────────────────────────────────
public sealed record ContentDetailDto(
    Guid Id,
    string Slug,
    string Status,
    string ContentType,
    Guid? AuthorId,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<ContentDetailTranslationDto> Translations,
    ContentDetailScheduleDto? Schedule);

public sealed record ContentDetailTranslationDto(
    string LanguageCode, string Title, string Body,
    string? MetaTitle, string? MetaDescription, bool IsPublished);

public sealed record ContentDetailScheduleDto(
    DateTime? PublishAt, DateTime? UnpublishAt,
    int? DurationMinutes, string? TimeZoneId);

// ── Public (slug) ──────────────────────────────────────────────────────────
public sealed record ContentDto(
    Guid Id, string Slug, string LanguageCode,
    string Title, string Body,
    string? MetaTitle, string? MetaDescription,
    string Status, DateTime? PublishedAt);

// ── With Sections ──────────────────────────────────────────────────────────
public sealed record ContentWithSectionsDto(
    Guid Id, string Slug, string Status,
    ScheduleDto? Schedule, List<SectionDto> Sections);

public sealed record SectionDto(
    Guid Id, string Name, int Order,
    bool IsVisible, bool IsEnabled,
    string? CssClass, Guid? ParentSectionId,
    AnimationDto? Animation, List<BlockDto> Blocks);

public sealed record AnimationDto(string? Type, int Duration, int Delay, string Easing);

public sealed record BlockDto(
    Guid Id, string BlockType, int Order, bool IsVisible,
    string Settings, List<BlockTranslationDto> Translations);

public sealed record BlockTranslationDto(
    string LanguageCode, string? Title, string? Body,
    string? AltText, string? LinkText);

public sealed record ScheduleDto(
    DateTime? PublishAt, DateTime? UnpublishAt,
    int? DurationMinutes, string TimeZoneId);

// ── CRUD ───────────────────────────────────────────────────────────────────
public sealed record CreateContentApiRequest(
    string Slug,
    string LanguageCode,
    string Title,
    string Body,
    string? MetaTitle = null,
    string? MetaDescription = null,
    string ContentType = "Page",
    Guid? ParentId = null);

public sealed record CreateContentApiResponse(Guid Id, string Slug);

public sealed record UpdateContentApiRequest(
    string LanguageCode, string Title, string Body,
    string? MetaTitle = null, string? MetaDescription = null);

public sealed record AddTranslationApiRequest(
    string LanguageCode, string Title, string Body,
    string? MetaTitle = null, string? MetaDescription = null);

public sealed record DuplicateContentRequest(string NewSlug);

public sealed record ContentTranslationDto(
    string LanguageCode, string Title, string Body,
    string? MetaTitle, string? MetaDescription, bool IsPublished);

// ── Bulk ───────────────────────────────────────────────────────────────────
public enum BulkActionType { Publish = 0, Archive = 1, SoftDelete = 2 }
public sealed record BulkActionRequest(List<Guid> ContentIds, BulkActionType Action);
public sealed record BulkActionResult(int Succeeded, int Failed, List<string> Errors);

public sealed record BulkLockRequest(
    List<Guid> ContentIds, bool IsLocked, string LockType = "Soft");

// ── Section ────────────────────────────────────────────────────────────────
public sealed record CreateSectionRequest(
    string Name, int Order, Guid? ParentSectionId = null,
    string? CssClass = null, string? AnimationType = null,
    int AnimationDuration = 300, int AnimationDelay = 0,
    string AnimationEasing = "ease-out");

public sealed record UpdateSectionRequest(
    string Name, int Order, bool IsVisible, bool IsEnabled,
    string? CssClass, string? AnimationType,
    int AnimationDuration = 300, int AnimationDelay = 0,
    string AnimationEasing = "ease-out");

public sealed record SectionOrderItem(Guid SectionId, int Order);

// ── Block ──────────────────────────────────────────────────────────────────
public sealed record UpsertBlockRequest(
    int BlockType, int Order, bool IsVisible, string Settings, Guid? BlockId = null);

public sealed record UpsertBlockTranslationRequest(
    string LanguageCode, string? Title, string? Body,
    string? AltText, string? LinkText);

// ── Schedule ───────────────────────────────────────────────────────────────
public sealed record SetScheduleRequest(
    DateTime? PublishAt,
    DateTime? UnpublishAt,
    int? DurationMinutes,
    string TimeZoneId = "UTC");

// ── Completion ─────────────────────────────────────────────────────────────
public sealed record ContentCompletionDto(
    Guid ContentId,
    string Slug,
    int OverallScore,
    List<string> RequiredLanguages,
    List<LangScoreDto> Languages,
    List<SectionScoreDto> Sections,
    bool CanPublish,
    List<string> CanPublishLanguages);

public sealed record LangScoreDto(string Code, int Score, List<string> MissingFields);
public sealed record SectionScoreDto(Guid SectionId, string Name, List<BlockScoreDto> Blocks);
public sealed record BlockScoreDto(Guid BlockId, string Type, int Score, List<string> MissingTranslations);

// ── Versions ───────────────────────────────────────────────────────────────
public sealed record ContentVersionDto(
    Guid Id, int VersionNumber, string? Label,
    bool IsPublished, Guid CreatedBy, DateTime CreatedAt);

// ── Trash ──────────────────────────────────────────────────────────────────
public sealed record TrashItemDto(
    Guid Id, string Slug, string Title,
    List<string> Languages, DateTime? DeletedAt);

// ── SEO ────────────────────────────────────────────────────────────────────
public sealed record ContentMetaDto(
    string Title, string Description,
    string? OgTitle, string? OgDescription, string? OgImageUrl,
    string CanonicalUrl, string Robots, string? StructuredDataJson,
    List<HreflangEntryDto> HreflangAlternates);

public sealed record HreflangEntryDto(string HrefLang, string Href);

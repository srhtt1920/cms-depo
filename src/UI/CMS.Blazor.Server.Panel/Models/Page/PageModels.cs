namespace CMS.Blazor.Server.Panel.Models.Page;

// ── Tree (Light) ───────────────────────────────────────────────────────────
public sealed record PageTreeNodeDto(
    Guid Id, Guid? ParentId,
    string PageType, string? ContentType,
    int Order, bool IsActive, bool IsVisible, string? Icon,
    List<PageTreeNodeDto> Children);

// ── Tree (Detail) ──────────────────────────────────────────────────────────
public sealed record PageTreeDetailNodeDto(
    Guid Id, Guid? ParentId,
    string PageType, string? ContentType,
    int Order, bool IsActive, bool IsVisible,
    string? Icon, string? ExternalUrl, Guid? LinkedContentId,
    List<PageTranslationResponseDto> Translations,
    List<PageTreeDetailNodeDto> Children);

public sealed record PageTranslationResponseDto(
    string LanguageCode, string Title, string LinkName, string Slug,
    string? MetaTitle, string? MetaDescription, string? MetaKeywords);

// ── Single Page ────────────────────────────────────────────────────────────
public sealed record PageDetailDto(
    Guid Id, Guid? ParentId,
    string PageType, string? ContentType,
    int Order, bool IsActive, bool IsVisible,
    string? Icon, string? ExternalUrl, Guid? LinkedContentId,
    DateTime CreatedAt, DateTime? UpdatedAt,
    List<PageTranslationDetailDto> Translations);

public sealed record PageTranslationDetailDto(
    string LanguageCode, string Title, string LinkName, string Slug,
    string? MetaTitle, string? MetaDescription, string? MetaKeywords);

// ── CRUD ───────────────────────────────────────────────────────────────────
public sealed record CreatePageRequest(
    string PageType, string? ContentType, Guid? ParentId, int Order,
    string LanguageCode, string Title, string LinkName, string Slug,
    string? MetaTitle = null, string? MetaDescription = null, string? MetaKeywords = null,
    Guid? LinkedContentId = null, string? Icon = null, string? ExternalUrl = null);

public sealed record CreatePageResponse(Guid Id);

public sealed record UpsertPageTranslationRequest(
    string LanguageCode, string Title, string LinkName, string Slug,
    string? MetaTitle, string? MetaDescription, string? MetaKeywords);

public sealed record MovePageApiRequest(Guid? NewParentId, int NewOrder);

public sealed record LinkContentApiRequest(Guid? ContentId);

public sealed record UpdatePageSettingsRequest(
    string PageType, bool IsActive, bool IsVisible,
    string? Icon, int Order, string? ExternalUrl);

public sealed record SetPageActiveApiRequest(bool IsActive);

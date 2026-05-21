using CMS.Domain.Common;
using CMS.Domain.Contents;
using CMS.Domain.Pages.Events;
using CMS.Domain.Tenants;

namespace CMS.Domain.Pages;

// <summary>
/// Site sayfası veya kategori. Self-referencing tree yapısı.
/// ContentId ile mevcut Content aggregate'e opsiyonel bağlantı kurulabilir.
/// </summary>
public sealed class Page : AggregateRoot<PageId>, ITenantEntity, ISoftDeletable
{
    private readonly List<PageTranslation> _translations = [];

    public TenantId TenantId { get; private set; } = default!;
    public PageType PageType { get; private set; }

    /// <summary>Yalnızca PageType.Normal için dolu; Category'de null.</summary>
    public ContentType? ContentType { get; private set; }

    /// <summary>Mevcut Content aggregate'e opsiyonel bağlantı.</summary>
    public ContentId? LinkedContentId { get; private set; }

    /// <summary>null = root node.</summary>
    public PageId? ParentId { get; private set; }

    /// <summary>Aynı parent altındaki sıralama.</summary>
    public int Order { get; private set; }

    public bool IsActive { get; private set; } = true;

    /// <summary>Menüde görünsün mü?</summary>
    public bool IsVisible { get; private set; } = true;

    /// <summary>CSS class veya ikon URL'i.</summary>
    public string? Icon { get; private set; }

    /// <summary>Dış URL ise burada tutulur; dahili sayfalarda null.</summary>
    public string? ExternalUrl { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    Guid ITenantEntity.TenantId => TenantId.Value;

    public IReadOnlyList<PageTranslation> Translations => _translations.AsReadOnly();

    private Page() { }

    // ── Factory — Normal Sayfa ────────────────────────────────────────────────
    public static Page CreateNormal(
        TenantId tenantId,
        ContentType contentType,
        string languageCode,
        string title,
        string linkName,
        string slug,
        PageId? parentId = null,
        int order = 0,
        ContentId? linkedContentId = null,
        string? icon = null,
        string? externalUrl = null)
    {
        var page = new Page
        {
            Id = PageId.New(),
            TenantId = tenantId,
            PageType = PageType.Default,
            ContentType = contentType,
            ParentId = parentId,
            Order = order,
            LinkedContentId = linkedContentId,
            Icon = icon,
            ExternalUrl = externalUrl,
            IsActive = true,
            IsVisible = true,
            CreatedAt = DateTime.UtcNow,
        };
        page._translations.Add(
            PageTranslation.Create(page.Id, languageCode, title, linkName, slug));
        page.AddDomainEvent(new PageCreatedEvent(page.Id, tenantId));
        return page;
    }

    // ── Factory — Kategori ────────────────────────────────────────────────────
    public static Page CreateCategory(
        TenantId tenantId,
        string languageCode,
        string title,
        string linkName,
        string slug,
        PageId? parentId = null,
        int order = 0,
        string? icon = null)
    {
        var page = new Page
        {
            Id = PageId.New(),
            TenantId = tenantId,
            PageType = PageType.Category,
            ContentType = null,
            ParentId = parentId,
            Order = order,
            Icon = icon,
            IsActive = true,
            IsVisible = true,
            CreatedAt = DateTime.UtcNow,
        };
        page._translations.Add(
            PageTranslation.Create(page.Id, languageCode, title, linkName, slug));
        page.AddDomainEvent(new PageCreatedEvent(page.Id, tenantId));
        return page;
    }

    // ── Çeviri ───────────────────────────────────────────────────────────────
    public void AddTranslation(
        string languageCode,
        string title,
        string linkName,
        string slug,
        string? metaTitle = null,
        string? metaDescription = null,
        string? metaKeywords = null)
    {
        if (_translations.Any(t => t.LanguageCode == languageCode.ToLowerInvariant()))
            throw new InvalidOperationException($"Translation for '{languageCode}' already exists.");

        _translations.Add(PageTranslation.Create(
            Id, languageCode, title, linkName, slug,
            metaTitle, metaDescription, metaKeywords));
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateTranslation(
        string languageCode,
        string title,
        string linkName,
        string slug,
        string? metaTitle,
        string? metaDescription,
        string? metaKeywords)
    {
        var translation = _translations
            .FirstOrDefault(t => t.LanguageCode == languageCode.ToLowerInvariant())
            ?? throw new InvalidOperationException($"Translation for '{languageCode}' not found.");

        translation.Update(title, linkName, slug, metaTitle, metaDescription, metaKeywords);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Sayfa genel ayarlarını günceller.
    /// PUT /api/pages/{id} — UpdatePageSettingsHandler tarafından çağrılır.
    /// </summary>
    public void UpdateSettings(
        bool isActive,
        bool isVisible,
        string? icon,
        int order,
        string? externalUrl)
    {
        IsActive = isActive;
        IsVisible = isVisible;
        Icon = icon;
        Order = order;
        ExternalUrl = externalUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    // ── Tree Operasyonları ────────────────────────────────────────────────────
    public void MoveTo(PageId? newParentId)
    {
        if (newParentId == Id)
            throw new InvalidOperationException("A page cannot be its own parent.");

        var oldParent = ParentId;
        ParentId = newParentId;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new PageMovedEvent(Id, TenantId, oldParent, newParentId));
    }

    public void Reorder(int newOrder)
    {
        Order = newOrder;
        UpdatedAt = DateTime.UtcNow;
    }

    // ── Visibility / Activation ───────────────────────────────────────────────
    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetVisible(bool isVisible)
    {
        IsVisible = isVisible;
        UpdatedAt = DateTime.UtcNow;
    }

    // ── Content Bağlantısı ───────────────────────────────────────────────────
    public void LinkContent(ContentId contentId)
    {
        if (PageType == PageType.Category)
            throw new InvalidOperationException("Category pages cannot be linked to a content.");

        LinkedContentId = contentId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UnlinkContent()
    {
        LinkedContentId = null;
        UpdatedAt = DateTime.UtcNow;
    }

    // ── Soft Delete ──────────────────────────────────────────────────────────
    public void SoftDelete()
    {
        DeletedAt = DateTime.UtcNow;
        AddDomainEvent(new PageDeletedEvent(Id, TenantId));
    }

    public void Restore() => DeletedAt = null;


}

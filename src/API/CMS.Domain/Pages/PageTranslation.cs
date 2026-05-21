using CMS.Domain.Common;

namespace CMS.Domain.Pages;

public sealed class PageTranslation : Entity<Guid>
{
    public PageId PageId { get; private set; } = default!;
    public string LanguageCode { get; private set; } = default!;

    /// <summary>Sayfa başlığı — dil bazlı.</summary>
    public string Title { get; private set; } = default!;

    /// <summary>Menü/breadcrumb kısa isim — dil bazlı.</summary>
    public string LinkName { get; private set; } = default!;

    /// <summary>URL slug — dil bazlı (/tr/hizmetler, /en/services).</summary>
    public string Slug { get; private set; } = default!;

    public string? MetaTitle { get; private set; }
    public string? MetaDescription { get; private set; }
    public string? MetaKeywords { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private PageTranslation() { } // EF Core

    public static PageTranslation Create(
        PageId pageId,
        string languageCode,
        string title,
        string linkName,
        string slug,
        string? metaTitle = null,
        string? metaDescription = null,
        string? metaKeywords = null)
    {
        return new PageTranslation
        {
            Id = Guid.NewGuid(),
            PageId = pageId,
            LanguageCode = languageCode.ToLowerInvariant(),
            Title = title,
            LinkName = linkName,
            Slug = slug.ToLowerInvariant().Trim('/'),
            MetaTitle = metaTitle,
            MetaDescription = metaDescription,
            MetaKeywords = metaKeywords,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void Update(
        string title,
        string linkName,
        string slug,
        string? metaTitle,
        string? metaDescription,
        string? metaKeywords)
    {
        Title = title;
        LinkName = linkName;
        Slug = slug.ToLowerInvariant().Trim('/');
        MetaTitle = metaTitle;
        MetaDescription = metaDescription;
        MetaKeywords = metaKeywords;
        UpdatedAt = DateTime.UtcNow;
    }
}
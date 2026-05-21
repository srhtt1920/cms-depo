using CMS.Domain.Common;

namespace CMS.Domain.Contents;

public sealed class ContentTranslation : Entity<Guid>
{
    public ContentId ContentId { get; private set; } = default!;
    public string LanguageCode { get; private set; } = default!;
    public string Title { get; private set; } = default!;
    public string Body { get; private set; } = default!;
    public string? MetaTitle { get; private set; }
    public string? MetaDescription { get; private set; }
    public bool IsPublished { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private ContentTranslation() { } // EF Core

    public static ContentTranslation Create(
        ContentId contentId,
        string languageCode,
        string title,
        string body,
        string? metaTitle = null,
        string? metaDescription = null)
    {
        return new ContentTranslation
        {
            Id = Guid.NewGuid(),
            ContentId = contentId,
            LanguageCode = languageCode.ToLowerInvariant(),
            Title = title,
            Body = body,
            MetaTitle = metaTitle,
            MetaDescription = metaDescription,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string title, string body, string? metaTitle, string? metaDescription)
    {
        Title = title;
        Body = body;
        MetaTitle = metaTitle;
        MetaDescription = metaDescription;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Publish() => IsPublished = true;
    public void Unpublish() => IsPublished = false;
}

// EF Core navigation — global query filter için gerekli
// public Content? Content { get; private set; }

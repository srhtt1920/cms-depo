using CMS.Domain.Common;

namespace CMS.Domain.Contents;

/// <summary>
/// Block içeriğinin dile özgü metin alanları.
/// Hangi alanların zorunlu olduğu BlockType'a göre belirlenir.
/// </summary>
public sealed class BlockTranslation : Entity<Guid>
{
    public ContentBlockId BlockId      { get; private set; } = default!;
    public string         LanguageCode { get; private set; } = default!;
    public string?        Title        { get; private set; }
    public string?        Body         { get; private set; }
    public string?        AltText      { get; private set; }   // Image erişilebilirliği
    public string?        LinkText     { get; private set; }   // Link/Action görünen metin
    public DateTime       CreatedAt    { get; private set; }
    public DateTime?      UpdatedAt    { get; private set; }

    private BlockTranslation() { }

    public static BlockTranslation Create(
        ContentBlockId blockId,
        string languageCode,
        string?  title    = null,
        string?  body     = null,
        string?  altText  = null,
        string?  linkText = null)
    {
        return new BlockTranslation
        {
            Id           = Guid.NewGuid(),
            BlockId      = blockId,
            LanguageCode = languageCode.ToLowerInvariant(),
            Title        = title,
            Body         = body,
            AltText      = altText,
            LinkText     = linkText,
            CreatedAt    = DateTime.UtcNow
        };
    }

    public void Update(string? title, string? body, string? altText, string? linkText)
    {
        Title     = title;
        Body      = body;
        AltText   = altText;
        LinkText  = linkText;
        UpdatedAt = DateTime.UtcNow;
    }
}

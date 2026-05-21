using CMS.Domain.Common;

namespace CMS.Domain.Contents;

/// <summary>
/// Section içindeki tip bazlı içerik bloğu.
/// Settings = JSON string (tip'e özgü ayarlar).
/// </summary>
public sealed class ContentBlock : Entity<ContentBlockId>
{
    private readonly List<BlockTranslation> _translations = [];

    public ContentSectionId            SectionId    { get; private set; } = default!;
    public BlockType                   BlockType    { get; private set; }
    public int                         Order        { get; private set; }
    public bool                        IsVisible    { get; private set; } = true;
    public string                      Settings     { get; private set; } = "{}";
    public DateTime                    CreatedAt    { get; private set; }
    public DateTime?                   UpdatedAt    { get; private set; }

    public IReadOnlyList<BlockTranslation> Translations => _translations.AsReadOnly();

    private ContentBlock() { }

    public static ContentBlock Create(
        ContentSectionId sectionId,
        BlockType blockType,
        int order,
        string settings = "{}")
    {
        return new ContentBlock
        {
            Id        = ContentBlockId.New(),
            SectionId = sectionId,
            BlockType = blockType,
            Order     = order,
            IsVisible = true,
            Settings  = settings,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateSettings(string settings, bool? isVisible = null)
    {
        Settings  = settings;
        if (isVisible.HasValue) IsVisible = isVisible.Value;
        UpdatedAt = DateTime.UtcNow;
    }

    internal void SetOrder(int order) { Order = order; UpdatedAt = DateTime.UtcNow; }

    public void UpsertTranslation(
        string languageCode,
        string? title, string? body, string? altText, string? linkText)
    {
        var existing = _translations
            .FirstOrDefault(t => t.LanguageCode == languageCode.ToLowerInvariant());

        if (existing is not null)
            existing.Update(title, body, altText, linkText);
        else
            _translations.Add(BlockTranslation.Create(Id, languageCode, title, body, altText, linkText));

        UpdatedAt = DateTime.UtcNow;
    }
}

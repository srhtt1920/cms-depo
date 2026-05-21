using CMS.Domain.Common;

namespace CMS.Domain.Contents;

/// <summary>
/// İçerik bölümü. ParentSectionId ile sınırsız ağaç yapısı desteklenir.
/// Closure table veya path enumeration ile sorgulanır (repository katmanında).
/// </summary>
public sealed class ContentSection : Entity<ContentSectionId>
{
    private readonly List<ContentBlock> _blocks = [];

    public ContentId               ContentId       { get; private set; } = default!;
    public ContentSectionId?       ParentSectionId { get; private set; }
    public string                  Name            { get; private set; } = default!;
    public int                     Order           { get; private set; }
    public bool                    IsVisible       { get; private set; } = true;
    public bool                    IsEnabled       { get; private set; } = true;
    public string?                 CssClass        { get; private set; }
    public SectionAnimationSettings Animation      { get; private set; } = SectionAnimationSettings.None;
    public DateTime                CreatedAt       { get; private set; }
    public DateTime?               UpdatedAt       { get; private set; }

    public IReadOnlyList<ContentBlock> Blocks => _blocks.AsReadOnly();

    private ContentSection() { }

    public static ContentSection Create(
        ContentId contentId,
        string name,
        int order,
        ContentSectionId? parentSectionId = null,
        string? cssClass = null,
        SectionAnimationSettings? animation = null)
    {
        return new ContentSection
        {
            Id              = ContentSectionId.New(),
            ContentId       = contentId,
            ParentSectionId = parentSectionId,
            Name            = name,
            Order           = order,
            IsVisible       = true,
            IsEnabled       = true,
            CssClass        = cssClass,
            Animation       = animation ?? SectionAnimationSettings.None,
            CreatedAt       = DateTime.UtcNow
        };
    }

    public ContentBlock AddBlock(BlockType type, int order, string settings = "{}")
    {
        var block = ContentBlock.Create(Id, type, order, settings);
        _blocks.Add(block);
        UpdatedAt = DateTime.UtcNow;
        return block;
    }

    public void RemoveBlock(ContentBlockId blockId)
    {
        var block = _blocks.FirstOrDefault(b => b.Id == blockId)
            ?? throw new InvalidOperationException($"Block {blockId.Value} not found.");
        _blocks.Remove(block);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(string name, int order, bool isVisible, bool isEnabled,
        string? cssClass, SectionAnimationSettings? animation)
    {
        Name      = name;
        Order     = order;
        IsVisible = isVisible;
        IsEnabled = isEnabled;
        CssClass  = cssClass;
        Animation = animation ?? SectionAnimationSettings.None;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReorderBlocks(IEnumerable<(ContentBlockId Id, int Order)> ordering)
    {
        foreach (var (id, order) in ordering)
        {
            var block = _blocks.FirstOrDefault(b => b.Id == id);
            if (block is not null) block.SetOrder(order);
        }
        UpdatedAt = DateTime.UtcNow;
    }
}

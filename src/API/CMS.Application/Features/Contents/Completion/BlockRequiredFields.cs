using CMS.Domain.Contents;

namespace CMS.Application.Features.Contents.Completion;

/// <summary>
/// Her BlockType için hangi çeviri alanlarının zorunlu olduğunu tanımlar.
/// Dil + zorunlu alan bazlı tamamlanma hesabı için kullanılır.
/// </summary>
public static class BlockRequiredFields
{
    private static readonly Dictionary<BlockType, string[]> _map = new()
    {
        [BlockType.Text]   = ["Title", "Body"],
        [BlockType.Image]  = ["AltText"],
        [BlockType.Slider] = ["Title"],
        [BlockType.Card]   = ["Title", "Body"],
        [BlockType.Link]   = ["LinkText"],
        [BlockType.Action] = ["LinkText"],
        [BlockType.Custom] = [],
    };

    public static IReadOnlyList<string> For(BlockType type) =>
        _map.TryGetValue(type, out var fields) ? fields : Array.Empty<string>();
}

namespace CMS.SharedKernel.Pagination;

public sealed class Paginate<T>
{
    public int Index { get; init; }
    public int Size { get; init; }
    public int Count { get; init; }
    public int Pages => (int)Math.Ceiling(Count / (double)Size);
    public bool HasPrevious => Index > 0;
    public bool HasNext => Index + 1 < Pages;
    public IList<T> Items { get; init; } = [];
}

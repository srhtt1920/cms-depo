namespace CMS.Domain.Pages;

public sealed record PageId(Guid Value)
{
    public static PageId New() => new(Guid.NewGuid());
    public static PageId From(Guid id) => new(id);

    public override string ToString() => Value.ToString();
}


namespace CMS.Domain.Contents;
public sealed record ContentBlockId(Guid Value)
{
    public static ContentBlockId New() => new(Guid.NewGuid());
    public static ContentBlockId From(Guid value) => new(value);
}

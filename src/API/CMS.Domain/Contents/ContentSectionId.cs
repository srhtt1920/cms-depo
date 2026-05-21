namespace CMS.Domain.Contents;
public sealed record ContentSectionId(Guid Value)
{
    public static ContentSectionId New() => new(Guid.NewGuid());
    public static ContentSectionId From(Guid value) => new(value);
}

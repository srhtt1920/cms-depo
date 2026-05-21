using CMS.Domain.Common;

namespace CMS.Domain.Contents;

/// <summary>
/// SEO meta verileri. Her dil çevirisi kendi SeoMeta'sını taşır.
/// </summary>
public sealed record SeoMeta : ValueObject
{
    public string? MetaTitle       { get; init; }
    public string? MetaDescription { get; init; }
    public string? OgTitle         { get; init; }
    public string? OgDescription   { get; init; }
    public string? OgImageUrl      { get; init; }
    public string? CanonicalUrl    { get; init; }
    public string  RobotsDirective { get; init; } = "index, follow";
    /// <summary>JSON-LD structured data (Article, BreadcrumbList, Product vb.)</summary>
    public string? StructuredDataJson { get; init; }

    public static SeoMeta Empty => new();
}

using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.Seo;

public sealed record GetContentMetaQuery(string Slug, string Lang) : IRequest<Result<ContentMetaDto>>;

public sealed record ContentMetaDto(
    string  Title,
    string  Description,
    string? OgTitle,
    string? OgDescription,
    string? OgImageUrl,
    string  CanonicalUrl,
    string  Robots,
    string? StructuredDataJson,
    IReadOnlyList<HreflangEntry> HreflangAlternates
);

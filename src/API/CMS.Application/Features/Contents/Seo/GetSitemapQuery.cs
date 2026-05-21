using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.Seo;

public sealed record GetSitemapQuery(string BaseUrl) : IRequest<Result<SitemapDto>>;

public sealed record SitemapDto(IReadOnlyList<SitemapEntry> Entries);

public sealed record SitemapEntry(
    string  Loc,
    string  LastMod,
    string  ChangeFreq,
    double  Priority,
    IReadOnlyList<HreflangEntry> Alternates
);

public sealed record HreflangEntry(string HrefLang, string Href);

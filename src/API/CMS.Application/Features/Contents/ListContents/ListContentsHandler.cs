using CMS.Application.Common.Abstractions;
using CMS.Domain.Contents;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Pagination;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.ListContents;

[RequirePermission("content.read")]
public sealed class ListContentsHandler(
    IContentRepository contentRepository,
    ILanguageContext languageContext)
    : IRequestHandler<ListContentsQuery, Result<ListContentsResponse>>
{
    public async Task<Result<ListContentsResponse>> Handle(
        ListContentsQuery request, CancellationToken ct)
    {
        var paginated = await contentRepository.GetListAsync(
            request.Search,
            request.Status,
            request.ContentType,   // ? GEÇÝLÝYOR
            request.PageIndex,
            request.PageSize,
            ct);

        var items = paginated.Items.Select(c =>
        {
            var translation = c.Translations
                .FirstOrDefault(t => t.LanguageCode == languageContext.RequestedLanguage)
                ?? c.Translations.FirstOrDefault();

            return new ContentSummaryDto(
                c.Id.Value,
                c.Slug.Value,
                c.Status.ToString(),
                c.ContentType.ToString(),
                translation?.Title ?? string.Empty,
                c.Translations.Select(t => t.LanguageCode).ToList(),
                c.CreatedAt,
                c.UpdatedAt);
        }).ToList();

        return new ListContentsResponse(new Paginate<ContentSummaryDto>
        {
            Index = paginated.Index,
            Size = paginated.Size,
            Count = paginated.Count,
            Items = items
        });
    }
}

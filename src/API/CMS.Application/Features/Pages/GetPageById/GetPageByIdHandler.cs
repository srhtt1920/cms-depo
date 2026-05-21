using CMS.Application.Common.Abstractions;
using CMS.Domain.Pages;
using CMS.Domain.Pages.Errors;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Pages.GetPageById;

public sealed record GetPageByIdQuery(Guid Id) : IRequest<Result<PageDetailDto>>;

[RequirePermission("page.read")]
public sealed class GetPageByIdHandler(
    IPageRepository pageRepository,
    ITenantContext tenantContext)
    : IRequestHandler<GetPageByIdQuery, Result<PageDetailDto>>
{
    public async Task<Result<PageDetailDto>> Handle(
        GetPageByIdQuery request, CancellationToken ct)
    {
        var page = await pageRepository.GetByIdAsync(PageId.From(request.Id), ct);

        if (page is null || page.TenantId.Value != tenantContext.TenantId)
            return Result.Failure<PageDetailDto>(PageErrors.NotFound(request.Id));

        return new PageDetailDto(
            page.Id.Value,
            page.ParentId?.Value,
            page.PageType.ToString(),
            page.ContentType?.ToString(),
            page.Order,
            page.IsActive,
            page.IsVisible,
            page.Icon,
            page.ExternalUrl,
            page.LinkedContentId?.Value,
            page.CreatedAt,
            page.UpdatedAt,
            page.Translations.Select(t => new PageTranslationDetailDto(
                t.LanguageCode, t.Title, t.LinkName, t.Slug,
                t.MetaTitle, t.MetaDescription, t.MetaKeywords)).ToList());
    }
}

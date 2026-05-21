using CMS.Domain.Pages;
using CMS.Domain.Pages.Errors;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Pages.GetPageBreadcrumb;

public sealed record GetPageBreadcrumbQuery(Guid Id, string LanguageCode = "tr")
  : IRequest<Result<List<BreadcrumbItemDto>>>;

public sealed record BreadcrumbItemDto(
    Guid Id,
    string Title,
    string Slug,
    int Depth);

public sealed class GetPageBreadcrumbHandler(IPageRepository pageRepository)
 : IRequestHandler<GetPageBreadcrumbQuery, Result<List<BreadcrumbItemDto>>>
{
    public async Task<Result<List<BreadcrumbItemDto>>> Handle(
        GetPageBreadcrumbQuery request, CancellationToken ct)
    {
        var page = await pageRepository.GetByIdAsync(PageId.From(request.Id), ct);
        if (page is null)
            return Result.Failure<List<BreadcrumbItemDto>>(PageErrors.NotFound(request.Id));

        var crumbs = await pageRepository.GetBreadcrumbAsync(
            page.Id, request.LanguageCode, ct);

        return crumbs
            .Select(c => new BreadcrumbItemDto(c.Id, c.Title, c.Slug, c.Depth))
            .ToList();
    }
}

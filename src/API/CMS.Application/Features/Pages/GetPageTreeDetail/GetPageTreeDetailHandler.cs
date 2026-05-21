using CMS.Application.Common.Abstractions;
using CMS.Domain.Pages;
using CMS.Domain.Pages.Dtos;
using CMS.Domain.Tenants;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Pages.GetPageTreeDetail;

public sealed record GetPageTreeDetailQuery(
    string LanguageCode = "tr",
    bool IncludeInactive = false)
    : IRequest<Result<List<PageTreeDetailNodeDto>>>;

[RequirePermission("page.read")]
public sealed class GetPageTreeDetailHandler(
    IPageRepository pageRepository,
    ITenantContext tenantContext)
    : IRequestHandler<GetPageTreeDetailQuery, Result<List<PageTreeDetailNodeDto>>>
{
    public async Task<Result<List<PageTreeDetailNodeDto>>> Handle(
        GetPageTreeDetailQuery request, CancellationToken ct)
    {
        var flat = await pageRepository.GetFlatDetailAsync(
            TenantId.From(tenantContext.TenantId), ct);

        var nodes = request.IncludeInactive
            ? flat
            : flat.Where(n => n.IsActive).ToList();

        return BuildTree(nodes, null);
    }

    private static List<PageTreeDetailNodeDto> BuildTree(
        IEnumerable<PageFlatDetailDto> nodes, Guid? parentId)
    {
        return nodes
            .Where(n => n.ParentId == parentId)
            .OrderBy(n => n.Order)
            .Select(n => new PageTreeDetailNodeDto(
                n.Id,
                n.ParentId,
                n.PageType,
                n.ContentType,
                n.Order,
                n.IsActive,
                n.IsVisible,
                n.Icon,
                n.ExternalUrl,
                n.LinkedContentId,
                n.Translations.Select(t => new PageTranslationResponseDto(
                    t.LanguageCode, t.Title, t.LinkName, t.Slug,
                    t.MetaTitle, t.MetaDescription, t.MetaKeywords)).ToList(),
                BuildTree(nodes, n.Id)))
            .ToList();
    }
}

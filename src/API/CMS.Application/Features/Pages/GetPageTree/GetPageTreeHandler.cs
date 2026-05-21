using CMS.Application.Common.Abstractions;
using CMS.Domain.Pages;
using CMS.Domain.Pages.Dtos;
using CMS.Domain.Tenants;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Pages.GetPageTree;

public sealed record GetPageTreeQuery(bool IncludeInactive = false) : IRequest<Result<List<PageTreeNodeDto>>>;

[RequirePermission("page.read")]
public sealed class GetPageTreeHandler(
 IPageRepository pageRepository,
 ITenantContext tenantContext)
 : IRequestHandler<GetPageTreeQuery, Result<List<PageTreeNodeDto>>>
{
    public async Task<Result<List<PageTreeNodeDto>>> Handle(
        GetPageTreeQuery request, CancellationToken ct)
    {
        var flat = await pageRepository.GetFlatAsync(
            TenantId.From(tenantContext.TenantId), ct);

        var nodes = request.IncludeInactive
            ? flat
            : flat.Where(n => n.IsActive).ToList();

        return BuildTree(nodes, null);
    }

    private static List<PageTreeNodeDto> BuildTree(
        IEnumerable<PageFlatDto> nodes, Guid? parentId)
    {
        return nodes
            .Where(n => n.ParentId == parentId)
            .OrderBy(n => n.Order)
            .Select(n => new PageTreeNodeDto(
                n.Id,
                n.ParentId,
                n.PageType,
                n.ContentType,
                n.Order,
                n.IsActive,
                n.IsVisible,
                n.Icon,
                BuildTree(nodes, n.Id)))
            .ToList();
    }
}

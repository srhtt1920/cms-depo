using CMS.Application.Common.Abstractions;
using CMS.Domain.Pages;
using CMS.Domain.Pages.Errors;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Pages.MovePage;

public sealed record MovePageCommand(
    Guid PageId,
    Guid? NewParentId,
    int NewOrder
) : IRequest<Result>;

[RequirePermission("page.update")]
public sealed class MovePageHandler(
    IPageRepository pageRepository,
    ITenantContext tenantContext)
    : IRequestHandler<MovePageCommand, Result>
{
    public async Task<Result> Handle(MovePageCommand request, CancellationToken ct)
    {
        var pageId = PageId.From(request.PageId);
        var page = await pageRepository.GetByIdAsync(pageId, ct);

        if (page is null || page.TenantId.Value != tenantContext.TenantId)
            return Result.Failure(PageErrors.NotFound(request.PageId));

        // Parent var mı?
        if (request.NewParentId.HasValue)
        {
            var newParentId = PageId.From(request.NewParentId.Value);

            var parent = await pageRepository.GetByIdAsync(newParentId, ct);
            if (parent is null || parent.TenantId.Value != tenantContext.TenantId)
                return Result.Failure(PageErrors.ParentNotFound(request.NewParentId.Value));

            // Döngüsel referans kontrolü: hedef parent, sayfanın kendi torunu olamaz
            var descendantIds = await pageRepository.GetDescendantIdsAsync(pageId, ct);
            if (descendantIds.Any(d => d.Value == request.NewParentId.Value))
                return Result.Failure(PageErrors.CircularReference);
        }

        page.MoveTo(request.NewParentId.HasValue
            ? PageId.From(request.NewParentId.Value)
            : null);

        page.Reorder(request.NewOrder);
        await pageRepository.UpdateAsync(page, ct);
        return Result.Success();
    }
}

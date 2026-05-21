using CMS.Application.Common.Abstractions;
using CMS.Domain.Pages;
using CMS.Domain.Pages.Errors;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Pages.DeletePage;

public sealed record DeletePageCommand(Guid Id) : IRequest<Result>;

[RequirePermission("page.delete")]
public sealed class DeletePageHandler(
IPageRepository pageRepository,
ITenantContext tenantContext)
: IRequestHandler<DeletePageCommand, Result>
{
    public async Task<Result> Handle(DeletePageCommand request, CancellationToken ct)
    {
        var page = await pageRepository.GetByIdAsync(PageId.From(request.Id), ct);

        if (page is null || page.TenantId.Value != tenantContext.TenantId)
            return Result.Failure(PageErrors.NotFound(request.Id));

        // Alt sayfaları var mı kontrol et
        var descendants = await pageRepository.GetDescendantIdsAsync(page.Id, ct);
        if (descendants.Any())
            return Result.Failure(Error.Validation(
                "Page.HasChildren",
                "Bu sayfa alt sayfalara sahip. Önce alt sayfaları silmek veya taşımak gerekiyor."));

        page.SoftDelete();
        await pageRepository.UpdateAsync(page, ct);
        return Result.Success();
    }
}

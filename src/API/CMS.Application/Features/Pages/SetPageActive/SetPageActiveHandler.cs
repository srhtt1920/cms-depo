using CMS.Application.Common.Abstractions;
using CMS.Domain.Pages;
using CMS.Domain.Pages.Errors;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Pages.SetPageActive;

public sealed record SetPageActiveCommand(Guid PageId, bool IsActive) : IRequest<Result>;

[RequirePermission("page.update")]
public sealed class SetPageActiveHandler(IPageRepository pageRepository, ITenantContext tenantContext)
    : IRequestHandler<SetPageActiveCommand, Result>
{
    public async Task<Result> Handle(SetPageActiveCommand request, CancellationToken ct)
    {
        var page = await pageRepository.GetByIdAsync(PageId.From(request.PageId), ct);
        if (page is null || page.TenantId.Value != tenantContext.TenantId)
            return Result.Failure(PageErrors.NotFound(request.PageId));

        page.SetActive(request.IsActive);
        await pageRepository.UpdateAsync(page, ct);
        return Result.Success();
    }
}

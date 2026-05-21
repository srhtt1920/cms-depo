using CMS.Application.Common.Abstractions;
using CMS.Domain.Pages;
using CMS.Domain.Tenants;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Pages.UpdatePageSettings;

public sealed record UpdatePageSettingsCommand(
    Guid PageId,
    bool IsActive,
    bool IsVisible,
    string? Icon,
    int Order,
    string? ExternalUrl
) : IRequest<Result>;

[RequirePermission("pages.manage")]
public sealed class UpdatePageSettingsHandler(
    IPageRepository pageRepository,
    ITenantContext tenantContext)
    : IRequestHandler<UpdatePageSettingsCommand, Result>
{
    public async Task<Result> Handle(UpdatePageSettingsCommand req, CancellationToken ct)
    {
        var page = await pageRepository.GetByIdAsync(PageId.From(req.PageId), ct);
        if (page is null)
            return Result.Failure(Error.NotFound("Page.NotFound", $"Page '{req.PageId}' not found."));

        if (page.TenantId != TenantId.From(tenantContext.TenantId))
            return Result.Failure(Error.Forbidden("Page.WrongTenant", "Access denied."));

        page.UpdateSettings(
            isActive: req.IsActive,
            isVisible: req.IsVisible,
            icon: req.Icon,
            order: req.Order,
            externalUrl: req.ExternalUrl);

        await pageRepository.UpdateAsync(page, ct);
        return Result.Success();
    }
}

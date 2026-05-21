using CMS.Application.Common.Abstractions;
using CMS.Domain.Contents;
using CMS.Domain.Pages;
using CMS.Domain.Pages.Errors;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Pages.LinkPageContent;

public sealed record LinkPageContentCommand(Guid PageId, Guid? ContentId) : IRequest<Result>;

// <summary>
/// ContentId null gelirse bağlantı koparılır (unlink).
/// </summary>
[RequirePermission("page.update")]
public sealed class LinkPageContentHandler(
    IPageRepository pageRepository,
    IContentRepository contentRepository,
    ITenantContext tenantContext)
    : IRequestHandler<LinkPageContentCommand, Result>
{
    public async Task<Result> Handle(LinkPageContentCommand request, CancellationToken ct)
    {
        var page = await pageRepository.GetByIdAsync(PageId.From(request.PageId), ct);
        if (page is null || page.TenantId.Value != tenantContext.TenantId)
            return Result.Failure(PageErrors.NotFound(request.PageId));

        if (page.PageType == PageType.Category)
            return Result.Failure(PageErrors.CannotLinkContentToCategory);

        if (request.ContentId.HasValue)
        {
            // Content var mı ve aynı tenant mı?
            var content = await contentRepository.GetByIdAsync(
                ContentId.From(request.ContentId.Value), ct);

            if (content is null || content.TenantId.Value != tenantContext.TenantId)
                return Result.Failure(Error.NotFound(
                    "Content.NotFound",
                    $"Content '{request.ContentId.Value}' was not found."));

            page.LinkContent(ContentId.From(request.ContentId.Value));
        }
        else
        {
            page.UnlinkContent();
        }

        await pageRepository.UpdateAsync(page, ct);
        return Result.Success();
    }
}

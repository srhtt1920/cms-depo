using CMS.Application.Common.Abstractions;
using CMS.Domain.Contents;
using CMS.Domain.Tenants;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.UnpublishContent;

[RequirePermission("content.publish")]
public sealed class UnpublishContentHandler(
    IContentRepository contentRepository,
    ITenantContext tenantContext)
    : IRequestHandler<UnpublishContentCommand, Result>
{
    public async Task<Result> Handle(UnpublishContentCommand request, CancellationToken ct)
    {
        var content = await contentRepository.GetByIdAsync(
            ContentId.From(request.ContentId), ct);

        if (content is null)
            return Result.Failure(Error.NotFound("Content.NotFound", "Content not found."));

        if (content.TenantId != TenantId.From(tenantContext.TenantId))
            return Result.Failure(Error.Forbidden("Content.WrongTenant", "Access denied."));

        try { content.Unpublish(); }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Validation("Content.CannotUnpublish", ex.Message));
        }

        await contentRepository.UpdateAsync(content, ct);
        return Result.Success();
    }
}

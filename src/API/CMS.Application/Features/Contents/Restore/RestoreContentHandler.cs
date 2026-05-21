using CMS.Application.Common.Abstractions;
using CMS.Domain.Contents;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.Restore;

[RequirePermission("content.restore")]
public sealed class RestoreContentHandler(IContentRepository repo, ITenantContext tenant)
    : IRequestHandler<RestoreContentCommand, Result>
{
    public async Task<Result> Handle(RestoreContentCommand req, CancellationToken ct)
    {
        var content = await repo.GetDeletedByIdAsync(ContentId.From(req.ContentId), ct);
        if (content is null)
            return Result.Failure(Error.NotFound("Content.NotFound", "Content not found in trash."));
        if (content.TenantId != CMS.Domain.Tenants.TenantId.From(tenant.TenantId))
            return Result.Failure(Error.Forbidden("Content.WrongTenant", "Access denied."));

        content.Restore();
        await repo.UpdateAsync(content, ct);
        return Result.Success();
    }
}

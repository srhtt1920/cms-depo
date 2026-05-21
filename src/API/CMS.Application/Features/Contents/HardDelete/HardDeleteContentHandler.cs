using CMS.Application.Common.Abstractions;
using CMS.Domain.Contents;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.HardDelete;

[RequirePermission("content.hardDelete")]
public sealed class HardDeleteContentHandler(IContentRepository repo, ITenantContext tenant)
    : IRequestHandler<HardDeleteContentCommand, Result>
{
    public async Task<Result> Handle(HardDeleteContentCommand req, CancellationToken ct)
    {
        // Sadece soft-deleted içerikler hard-delete edilebilir (çöp kutusu akışı)
        var content = await repo.GetDeletedByIdAsync(ContentId.From(req.ContentId), ct);
        if (content is null)
            return Result.Failure(Error.NotFound("Content.NotFound",
                "Content not found or not in trash."));
        if (content.TenantId != CMS.Domain.Tenants.TenantId.From(tenant.TenantId))
            return Result.Failure(Error.Forbidden("Content.WrongTenant", "Access denied."));

        await repo.HardDeleteAsync(content.Id, ct);
        return Result.Success();
    }
}

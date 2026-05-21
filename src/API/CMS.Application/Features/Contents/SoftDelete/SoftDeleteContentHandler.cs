using CMS.Application.Common.Abstractions;
using CMS.Domain.Contents;
using CMS.Domain.Tenants;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.SoftDelete;

[RequirePermission("content.delete")]
public sealed class SoftDeleteContentHandler(
    IContentRepository contentRepository,
    ITenantContext tenantContext)
    : IRequestHandler<SoftDeleteContentCommand, Result>
{
    public async Task<Result> Handle(SoftDeleteContentCommand request, CancellationToken ct)
    {
        var content = await contentRepository.GetByIdAsync(
            ContentId.From(request.ContentId), ct);

        if (content is null)
            return Result.Failure(Error.NotFound("Content.NotFound",
                $"Content '{request.ContentId}' not found."));

        if (content.TenantId != TenantId.From(tenantContext.TenantId))
            return Result.Failure(Error.Forbidden("Content.WrongTenant", "Access denied."));

        content.SoftDelete();
        await contentRepository.UpdateAsync(content, ct);
        return Result.Success();
    }
}

using CMS.Application.Common.Abstractions;
using CMS.Domain.Contents;
using CMS.Domain.Contents.Versioning;
using CMS.Domain.Tenants;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.GetVersions;

[RequirePermission("content.detailRead")]
public sealed class GetVersionsHandler(
IContentVersionRepository versionRepository,
IContentRepository contentRepository,
ITenantContext tenantContext)
: IRequestHandler<GetVersionsQuery, Result<List<ContentVersionDto>>>
{
    public async Task<Result<List<ContentVersionDto>>> Handle(
        GetVersionsQuery request, CancellationToken ct)
    {
        var content = await contentRepository.GetByIdAsync(
            ContentId.From(request.ContentId), ct);
        if (content is null)
            return Result.Failure<List<ContentVersionDto>>(
                Error.NotFound("Content.NotFound", "Content not found."));
        if (content.TenantId != TenantId.From(tenantContext.TenantId))
            return Result.Failure<List<ContentVersionDto>>(
                Error.Forbidden("Content.WrongTenant", "Access denied."));

        var versions = await versionRepository.GetByContentIdAsync(
            ContentId.From(request.ContentId), ct);

        return versions
            .OrderByDescending(v => v.VersionNumber)
            .Select(v => new ContentVersionDto(
                v.Id, v.VersionNumber, v.Label,
                v.IsPublished, v.CreatedBy.Value, v.CreatedAt))
            .ToList();
    }
}

using CMS.Application.Common.Abstractions;
using CMS.Domain.Contents;
using CMS.Domain.Tenants;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.BulkAction;

[RequirePermission("content.edit")]
public sealed class BulkActionHandler(
    IContentRepository contentRepository,
    ITenantContext tenantContext)
    : IRequestHandler<BulkActionCommand, Result<BulkActionResult>>
{
    public async Task<Result<BulkActionResult>> Handle(
        BulkActionCommand request, CancellationToken ct)
    {
        if (request.ContentIds.Count == 0)
            return Result.Failure<BulkActionResult>(
                Error.Validation("BulkAction.Empty", "At least one content ID required."));

        if (request.ContentIds.Count > 100)
            return Result.Failure<BulkActionResult>(
                Error.Validation("BulkAction.TooMany", "Maximum 100 items per bulk action."));

        var tenantId = TenantId.From(tenantContext.TenantId);
        int succeeded = 0;
        var errors = new List<string>();

        foreach (var id in request.ContentIds)
        {
            try
            {
                var content = await contentRepository.GetByIdAsync(ContentId.From(id), ct);
                if (content is null || content.TenantId != tenantId)
                {
                    errors.Add($"{id}: not found");
                    continue;
                }

                switch (request.Action)
                {
                    case BulkActionType.Publish:
                        content.Publish(); break;
                    case BulkActionType.Archive:
                        content.Archive(); break;
                    case BulkActionType.SoftDelete:
                        content.SoftDelete(); break;
                }

                await contentRepository.UpdateAsync(content, ct);
                succeeded++;
            }
            catch (Exception ex)
            {
                errors.Add($"{id}: {ex.Message}");
            }
        }

        return new BulkActionResult(succeeded, errors.Count, errors);
    }
}

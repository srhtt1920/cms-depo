using CMS.Application.Common.Abstractions;
using CMS.Domain.Contents;
using CMS.Domain.Tenants;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.Sections.ReorderSection;

[RequirePermission("content.edit")]
public sealed class ReorderSectionsHandler(
   IContentRepository contentRepository,
   ITenantContext tenantContext)
   : IRequestHandler<ReorderSectionsCommand, Result>
{
    public async Task<Result> Handle(ReorderSectionsCommand request, CancellationToken ct)
    {
        var content = await contentRepository.GetByIdWithSectionsAsync(
            ContentId.From(request.ContentId), ct);

        if (content is null)
            return Result.Failure(Error.NotFound("Content.NotFound", "Content not found."));

        if (content.TenantId != TenantId.From(tenantContext.TenantId))
            return Result.Failure(Error.Forbidden("Content.WrongTenant", "Access denied."));

        //content.ReorderSections(
        //    request.Items.Select(i => (ContentSectionId.From(i.SectionId), i.Order)));

        await contentRepository.UpdateAsync(content, ct);
        return Result.Success();
    }
}

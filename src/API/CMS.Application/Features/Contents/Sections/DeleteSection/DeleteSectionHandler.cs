using CMS.Application.Common.Abstractions;
using CMS.Domain.Contents;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.Sections.DeleteSection;

[RequirePermission("content.edit")]
public sealed class DeleteSectionHandler(IContentRepository repo, ITenantContext tenant)
    : IRequestHandler<DeleteSectionCommand, Result>
{
    public async Task<Result> Handle(DeleteSectionCommand req, CancellationToken ct)
    {
        var content = await repo.GetByIdWithSectionsAsync(ContentId.From(req.ContentId), ct);
        if (content is null) return Result.Failure(Error.NotFound("Content.NotFound", "Content not found."));
        content.RemoveSection(ContentSectionId.From(req.SectionId));
        await repo.UpdateAsync(content, ct);
        return Result.Success();
    }
}

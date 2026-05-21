using CMS.Application.Common.Abstractions;
using CMS.Domain.Contents;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.Blocks.DeleteBlock;

[RequirePermission("content.edit")]
public sealed class DeleteBlockHandler(IContentRepository repo, ITenantContext tenant)
    : IRequestHandler<DeleteBlockCommand, Result>
{
    public async Task<Result> Handle(DeleteBlockCommand req, CancellationToken ct)
    {
        var content = await repo.GetByIdWithSectionsAsync(ContentId.From(req.ContentId), ct);
        if (content is null || content.TenantId.Value != tenant.TenantId)
            return Result.Failure(Error.NotFound("Content.NotFound", "Content not found."));

        var section = content.Sections
            .FirstOrDefault(s => s.Id == ContentSectionId.From(req.SectionId));
        if (section is null)
            return Result.Failure(Error.NotFound("Section.NotFound", "Section not found."));

        var block = section.Blocks
            .FirstOrDefault(b => b.Id == ContentBlockId.From(req.BlockId));
        if (block is null)
            return Result.Failure(Error.NotFound("Block.NotFound", "Block not found."));

        section.RemoveBlock(ContentBlockId.From(req.BlockId));
        await repo.UpdateAsync(content, ct);
        return Result.Success();
    }
}

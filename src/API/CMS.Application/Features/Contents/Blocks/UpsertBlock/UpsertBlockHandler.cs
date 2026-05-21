using CMS.Application.Common.Abstractions;
using CMS.Domain.Contents;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.Blocks.UpsertBlock;

[RequirePermission("content.edit")]
public sealed class UpsertBlockHandler(IContentRepository repo, ITenantContext tenant)
    : IRequestHandler<UpsertBlockCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UpsertBlockCommand req, CancellationToken ct)
    {
        var content = await repo.GetByIdWithSectionsAsync(ContentId.From(req.ContentId), ct);
        if (content is null) return Result.Failure<Guid>(Error.NotFound("Content.NotFound", "Content not found."));

        var section = content.Sections.FirstOrDefault(s => s.Id == ContentSectionId.From(req.SectionId));
        if (section is null) return Result.Failure<Guid>(Error.NotFound("Section.NotFound", "Section not found."));

        if (req.BlockId.HasValue)
        {
            // Update
            var block = section.Blocks.FirstOrDefault(b => b.Id == ContentBlockId.From(req.BlockId.Value));
            if (block is null) return Result.Failure<Guid>(Error.NotFound("Block.NotFound", "Block not found."));
            block.UpdateSettings(req.Settings, req.IsVisible);
            await repo.UpdateAsync(content, ct);
            return block.Id.Value;
        }
        else
        {
            // Create
            var block = section.AddBlock(req.BlockType, req.Order, req.Settings);
            await repo.UpdateAsync(content, ct);
            return block.Id.Value;
        }
    }
}

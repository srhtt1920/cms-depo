using CMS.Application.Common.Abstractions;
using CMS.Domain.Contents;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.Blocks.UpsertBlockTranslation;

[RequirePermission("content.edit")]
public sealed class UpsertBlockTranslationHandler(IContentRepository repo, ITenantContext tenant)
    : IRequestHandler<UpsertBlockTranslationCommand, Result>
{
    public async Task<Result> Handle(UpsertBlockTranslationCommand req, CancellationToken ct)
    {
        var content = await repo.GetByIdWithSectionsAsync(ContentId.From(req.ContentId), ct);
        if (content is null) return Result.Failure(Error.NotFound("Content.NotFound", "Content not found."));

        var section = content.Sections.FirstOrDefault(s => s.Id == ContentSectionId.From(req.SectionId));
        if (section is null) return Result.Failure(Error.NotFound("Section.NotFound", "Section not found."));

        var block = section.Blocks.FirstOrDefault(b => b.Id == ContentBlockId.From(req.BlockId));
        if (block is null) return Result.Failure(Error.NotFound("Block.NotFound", "Block not found."));

        block.UpsertTranslation(req.LanguageCode, req.Title, req.Body, req.AltText, req.LinkText);
        await repo.UpdateAsync(content, ct);
        return Result.Success();
    }
}

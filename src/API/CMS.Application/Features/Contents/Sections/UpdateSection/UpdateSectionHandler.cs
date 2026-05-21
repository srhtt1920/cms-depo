using CMS.Application.Common.Abstractions;
using CMS.Domain.Contents;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.Sections.UpdateSection;

[RequirePermission("content.edit")]
public sealed class UpdateSectionHandler(IContentRepository repo, ITenantContext tenant)
    : IRequestHandler<UpdateSectionCommand, Result>
{
    public async Task<Result> Handle(UpdateSectionCommand req, CancellationToken ct)
    {
        var content = await repo.GetByIdWithSectionsAsync(ContentId.From(req.ContentId), ct);
        if (content is null) return Result.Failure(Error.NotFound("Content.NotFound", "Content not found."));

        var section = content.Sections.FirstOrDefault(s => s.Id == ContentSectionId.From(req.SectionId));
        if (section is null) return Result.Failure(Error.NotFound("Section.NotFound", "Section not found."));

        var animation = req.AnimationType is null ? null : new SectionAnimationSettings
        {
            Type     = req.AnimationType,
            Duration = req.AnimationDuration,
            Delay    = req.AnimationDelay,
            Easing   = req.AnimationEasing
        };
        section.Update(req.Name, req.Order, req.IsVisible, req.IsEnabled, req.CssClass, animation);
        await repo.UpdateAsync(content, ct);
        return Result.Success();
    }
}

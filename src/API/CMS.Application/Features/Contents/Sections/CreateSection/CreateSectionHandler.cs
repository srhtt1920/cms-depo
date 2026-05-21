using CMS.Application.Common.Abstractions;
using CMS.Domain.Contents;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.Sections.CreateSection;

[RequirePermission("content.edit")]
public sealed class CreateSectionHandler(IContentRepository repo, ITenantContext tenant)
    : IRequestHandler<CreateSectionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateSectionCommand req, CancellationToken ct)
    {
        var content = await repo.GetByIdAsync(ContentId.From(req.ContentId), ct);
        if (content is null) return Result.Failure<Guid>(Error.NotFound("Content.NotFound", "Content not found."));
        if (content.TenantId != CMS.Domain.Tenants.TenantId.From(tenant.TenantId))
            return Result.Failure<Guid>(Error.Forbidden("Content.WrongTenant", "Access denied."));

        ContentSectionId? parentId = req.ParentSectionId.HasValue
            ? ContentSectionId.From(req.ParentSectionId.Value) : null;

        var animation = req.AnimationType is null ? null : new SectionAnimationSettings
        {
            Type     = req.AnimationType,
            Duration = req.AnimationDuration,
            Delay    = req.AnimationDelay,
            Easing   = req.AnimationEasing
        };

        var section = content.AddSection(req.Name, req.Order, parentId, req.CssClass, animation);
        await repo.UpdateAsync(content, ct);
        return section.Id.Value;
    }
}

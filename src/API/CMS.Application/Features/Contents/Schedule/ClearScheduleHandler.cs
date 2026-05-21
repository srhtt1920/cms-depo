using CMS.Application.Common.Abstractions;
using CMS.Domain.Contents;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.Schedule;

[RequirePermission("content.publish")]
public sealed class ClearScheduleHandler(IContentRepository repo, ITenantContext tenant)
    : IRequestHandler<ClearScheduleCommand, Result>
{
    public async Task<Result> Handle(ClearScheduleCommand req, CancellationToken ct)
    {
        var content = await repo.GetByIdAsync(ContentId.From(req.ContentId), ct);
        if (content is null) return Result.Failure(Error.NotFound("Content.NotFound", "Content not found."));
        content.ClearSchedule();
        await repo.UpdateAsync(content, ct);
        return Result.Success();
    }
}

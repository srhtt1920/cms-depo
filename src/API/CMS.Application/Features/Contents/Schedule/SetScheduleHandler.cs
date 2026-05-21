using CMS.Application.Common.Abstractions;
using CMS.Domain.Contents;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.Schedule;

[RequirePermission("content.publish")]
public sealed class SetScheduleHandler(IContentRepository repo, ITenantContext tenant)
    : IRequestHandler<SetScheduleCommand, Result>
{
    public async Task<Result> Handle(SetScheduleCommand req, CancellationToken ct)
    {
        var content = await repo.GetByIdAsync(ContentId.From(req.ContentId), ct);
        if (content is null) return Result.Failure(Error.NotFound("Content.NotFound", "Content not found."));
        if (content.TenantId != CMS.Domain.Tenants.TenantId.From(tenant.TenantId))
            return Result.Failure(Error.Forbidden("Content.WrongTenant", "Access denied."));

        try
        {
            var schedule = PublishSchedule.Create(
                req.PublishAt, req.UnpublishAt, req.DurationMinutes, req.TimeZoneId);
            content.SetSchedule(schedule);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(Error.Validation("Schedule.Invalid", ex.Message));
        }

        await repo.UpdateAsync(content, ct);
        return Result.Success();
    }
}

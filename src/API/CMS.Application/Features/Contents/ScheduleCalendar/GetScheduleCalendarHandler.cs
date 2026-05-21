using CMS.Application.Common.Abstractions;
using CMS.Domain.Contents;
using CMS.Domain.Tenants;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.ScheduleCalendar;

public sealed record GetScheduleCalendarQuery(
    DateTime From,
    DateTime To)
    : IRequest<Result<List<ScheduleCalendarItemDto>>>;

public sealed record ScheduleCalendarItemDto(
    Guid ContentId,
    string Slug,
    string Title,
    string ContentType,
    DateTime PublishAt,
    DateTime? UnpublishAt,
    string Status);

[RequirePermission("content.read")]
public sealed class GetScheduleCalendarHandler(
    IContentRepository contentRepo,
    ITenantContext tenantContext)
    : IRequestHandler<GetScheduleCalendarQuery, Result<List<ScheduleCalendarItemDto>>>
{
    public async Task<Result<List<ScheduleCalendarItemDto>>> Handle(
        GetScheduleCalendarQuery req, CancellationToken ct)
    {
        var tenantId = TenantId.From(tenantContext.TenantId);

        // Tarih aralığına giren tüm zamanlanmış içerikleri getir
        var scheduled = await contentRepo.GetScheduledInRangeAsync(
            tenantId, req.From, req.To, ct);

        var dtos = scheduled.Select(c =>
        {
            var title = c.Translations.FirstOrDefault()?.Title ?? c.Slug.Value;
            return new ScheduleCalendarItemDto(
                c.Id.Value,
                c.Slug.Value,
                title,
                c.ContentType.ToString(),
                c.Schedule!.PublishAt!.Value,
                c.Schedule.UnpublishAt,
                c.Status.ToString());
        }).ToList();

        return dtos;
    }
}
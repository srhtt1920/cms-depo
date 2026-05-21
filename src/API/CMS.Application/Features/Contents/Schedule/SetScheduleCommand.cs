using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.Schedule;

public sealed record SetScheduleCommand(
    Guid      ContentId,
    DateTime? PublishAt,
    DateTime? UnpublishAt,
    int?      DurationMinutes,
    string    TimeZoneId = "UTC"
) : IRequest<Result>;

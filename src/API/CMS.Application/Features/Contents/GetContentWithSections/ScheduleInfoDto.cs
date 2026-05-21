namespace CMS.Application.Features.Contents.GetContentWithSections;

public sealed record ScheduleInfoDto(
    DateTime? PublishAt,
    DateTime? UnpublishAt,
    int? DurationMinutes,
    string TimeZoneId);

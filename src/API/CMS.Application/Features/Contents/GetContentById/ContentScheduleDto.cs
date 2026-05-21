namespace CMS.Application.Features.Contents.GetContentById;

public sealed record ContentScheduleDto(
    DateTime? PublishAt,
    DateTime? UnpublishAt,
    int? DurationMinutes,
    string? TimeZoneId);

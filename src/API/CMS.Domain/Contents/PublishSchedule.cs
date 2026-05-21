using CMS.Domain.Common;

namespace CMS.Domain.Contents;

/// <summary>
/// İçerik yayın zamanlaması. Immutable value object.
/// DurationMinutes set edilirse UnpublishAt otomatik hesaplanır.
/// </summary>
public sealed record PublishSchedule : ValueObject
{
    public DateTime? PublishAt { get; private set; }
    public DateTime? UnpublishAt { get; private set; }
    public int? DurationMinutes { get; private set; }
    public string TimeZoneId { get; private set; } = "UTC";

    private PublishSchedule() { } 

    public static PublishSchedule Create(
        DateTime?  publishAt,
        DateTime?  unpublishAt,
        int?       durationMinutes,
        string     timeZoneId = "UTC")
    {
        if (publishAt.HasValue && unpublishAt.HasValue && unpublishAt <= publishAt)
            throw new ArgumentException("UnpublishAt must be after PublishAt.");

        DateTime? resolvedUnpublish = unpublishAt;
        if (durationMinutes.HasValue && publishAt.HasValue && resolvedUnpublish is null)
            resolvedUnpublish = publishAt.Value.AddMinutes(durationMinutes.Value);

        return new PublishSchedule
        {
            PublishAt = publishAt,
            UnpublishAt = resolvedUnpublish,
            DurationMinutes = durationMinutes,
            TimeZoneId = timeZoneId
        };
    }

    public bool IsLiveNow(DateTime utcNow) =>
        (PublishAt is null || PublishAt <= utcNow) &&
        (UnpublishAt is null || UnpublishAt > utcNow);

    public bool IsScheduled(DateTime utcNow) =>
        PublishAt.HasValue && PublishAt > utcNow;

    public bool IsExpired(DateTime utcNow) =>
        UnpublishAt.HasValue && UnpublishAt <= utcNow;
}

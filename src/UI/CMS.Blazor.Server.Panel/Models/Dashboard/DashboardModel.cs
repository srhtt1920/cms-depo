namespace CMS.Blazor.Server.Panel.Models.Dashboard;

public sealed record DashboardStatsDto(
    int TotalContents,
    int PublishedContents,
    int DraftContents,
    int ArchivedContents,
    int ScheduledContents,
    int TotalUsers,
    int ActiveUsers,
    int TotalPages,
    int ScheduledPublications,
    Dictionary<string, int> ContentsByType,
    List<DashboardActivityDto> RecentActivity);

public sealed record DashboardActivityDto(
    string Action, string EntityType,
    string? EntityId, string UserEmail, DateTime OccurredAt)
{
    /// <summary>Aktivite tipiyle eşleşen renk kodu (UI için).</summary>
    public string Color => Action?.ToLowerInvariant() switch
    {
        "created" => "#22c55e",
        "updated" => "#3b82f6",
        "deleted" => "#ef4444",
        "published" => "#8b5cf6",
        "archived" => "#f59e0b",
        _ => "#6b7280"
    };

    /// <summary>Dashboard aktivite satırında gösterilecek metin.</summary>
    public string Text => $"{UserEmail} — {Action} ({EntityType}{(EntityId is not null ? $" #{EntityId[..Math.Min(8, EntityId.Length)]}" : "")})";

    /// <summary>İnsan dostu zaman ifadesi.</summary>
    public string When
    {
        get
        {
            var diff = DateTime.UtcNow - OccurredAt;
            if (diff.TotalMinutes < 1) return "Az önce";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} dk önce";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} sa önce";
            return OccurredAt.ToString("dd MMM yyyy HH:mm");
        }
    }
}

public sealed record ScheduleCalendarItemDto(
    Guid ContentId, string Slug, string Title, string ContentType,
    DateTime PublishAt, DateTime? UnpublishAt, string Status);
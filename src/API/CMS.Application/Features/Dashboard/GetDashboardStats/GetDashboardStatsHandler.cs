using CMS.Application.Common.Abstractions;
using CMS.Domain.Common;
using CMS.Domain.Contents;
using CMS.Domain.Identity;
using CMS.Domain.Pages;
using CMS.Domain.Tenants;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Dashboard.GetDashboardStats;

public sealed record GetDashboardStatsQuery : IRequest<Result<DashboardStatsDto>>;

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
    List<RecentActivityDto> RecentActivity);

public sealed record RecentActivityDto(
    string Action,
    string EntityType,
    string? EntityId,
    string UserEmail,
    DateTime OccurredAt);

[RequirePermission("content.read")]
public sealed class GetDashboardStatsHandler(
    IContentRepository contentRepo,
    IUserRepository userRepo,
    IPageRepository pageRepo,
    IAuditLogRepository auditLogRepo,
    ITenantContext tenantContext)
    : IRequestHandler<GetDashboardStatsQuery, Result<DashboardStatsDto>>
{
    public async Task<Result<DashboardStatsDto>> Handle(
        GetDashboardStatsQuery _, CancellationToken ct)
    {
        var tenantId = TenantId.From(tenantContext.TenantId);

        // İçerik sayıları — tüm statüsleri tek sorguda al
        var allContents = (await contentRepo.GetListAsync(
            search: null, status: null, contentType: null,
            pageIndex: 0, pageSize: int.MaxValue, ct)).Items;

        var published = allContents.Count(c => c.Status == ContentStatus.Published);
        var draft = allContents.Count(c => c.Status == ContentStatus.Draft);
        var archived = allContents.Count(c => c.Status == ContentStatus.Archived);
        var scheduled = allContents.Count(c => c.Status == ContentStatus.Scheduled);

        var byType = allContents
            .GroupBy(c => c.ContentType.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        // Kullanıcı sayıları
        var users = await userRepo.GetByTenantAsync(tenantId, ct);
        var activeUsers = users.Count(u => u.IsActive);

        // Sayfa sayısı
        var pageTree = await pageRepo.GetTreeAsync(tenantId, includeInactive: true, ct);
        var pageCount = pageTree.Count;

        // Son 10 audit log
        var (recentLogs, _) = await auditLogRepo.GetPagedAsync(
            tenantId.Value, null, null, null, 0, 10, ct);

        var recentActivity = recentLogs.Select(l => new RecentActivityDto(
            l.Action, l.EntityType, l.EntityId,
            l.UserEmail, l.CreatedAt)).ToList();

        return new DashboardStatsDto(
            TotalContents: allContents.Count,
            PublishedContents: published,
            DraftContents: draft,
            ArchivedContents: archived,
            ScheduledContents: scheduled,
            TotalUsers: users.Count,
            ActiveUsers: activeUsers,
            TotalPages: pageCount,
            ScheduledPublications: scheduled,
            ContentsByType: byType,
            RecentActivity: recentActivity);
    }
}

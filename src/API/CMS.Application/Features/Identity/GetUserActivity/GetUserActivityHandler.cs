using CMS.Application.Common.Abstractions;
using CMS.Domain.Common;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Identity.GetUserActivity;

public sealed record GetUserActivityQuery(
    Guid UserId,
    int PageIndex = 0,
    int PageSize = 30)
    : IRequest<Result<UserActivityPageDto>>;

public sealed record UserActivityPageDto(
    int TotalCount,
    List<UserActivityItemDto> Items);

public sealed record UserActivityItemDto(
    Guid Id,
    string Action,
    string EntityType,
    string? EntityId,
    string? Details,
    string? IpAddress,
    string Icon,           // UI için ikon sınıfı
    string Color,          // UI için renk kodu
    DateTime OccurredAt);

[RequirePermission("users.manage")]
public sealed class GetUserActivityHandler(
    IAuditLogRepository auditLogRepo,
    ITenantContext tenantContext)
    : IRequestHandler<GetUserActivityQuery, Result<UserActivityPageDto>>
{
    // Action → Icon ve Color mapping
    private static readonly Dictionary<string, (string Icon, string Color)> ActionMap = new()
    {
        ["Content.Created"] = ("bi-file-plus", "#4CAF50"),
        ["Content.Published"] = ("bi-globe", "#2196F3"),
        ["Content.Updated"] = ("bi-pencil", "#FF9800"),
        ["Content.Deleted"] = ("bi-trash", "#F44336"),
        ["Content.Approved"] = ("bi-check-circle", "#4CAF50"),
        ["Content.Rejected"] = ("bi-x-circle", "#F44336"),
        ["User.Login"] = ("bi-box-arrow-in-right", "#9C27B0"),
        ["User.Logout"] = ("bi-box-arrow-right", "#9E9E9E"),
    };

    public async Task<Result<UserActivityPageDto>> Handle(
        GetUserActivityQuery req, CancellationToken ct)
    {
        var (items, total) = await auditLogRepo.GetPagedAsync(
            tenantId: tenantContext.TenantId,
            userId: req.UserId,
            entityType: null,
            action: null,
            pageIndex: req.PageIndex,
            pageSize: req.PageSize,
            ct);

        var dtos = items.Select(log =>
        {
            var (icon, color) = ActionMap.TryGetValue(log.Action, out var style)
                ? style : ("bi-activity", "#607D8B");
            return new UserActivityItemDto(
                log.Id, log.Action, log.EntityType,
                log.EntityId, log.Details, log.IpAddress,
                icon, color, log.CreatedAt);
        }).ToList();

        return new UserActivityPageDto(total, dtos);
    }
}


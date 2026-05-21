using CMS.Domain.Common;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Pagination;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.SuperAdmin.GetSuperAdminAudit;

public sealed record GetSuperAdminAuditQuery(
    int PageIndex = 0,
    int PageSize = 50)
    : IRequest<Result<Paginate<AuditLogs.AuditLogDto>>>;

[RequirePermission("superadmin")]
public sealed class GetSuperAdminAuditHandler(IAuditLogRepository auditLogRepository)
    : IRequestHandler<GetSuperAdminAuditQuery, Result<Paginate<AuditLogs.AuditLogDto>>>
{
    public async Task<Result<Paginate<AuditLogs.AuditLogDto>>> Handle(
        GetSuperAdminAuditQuery req, CancellationToken ct)
    {
        // SuperAdmin eylemlerine filtrelenmiş audit log
        var (items, total) = await auditLogRepository.GetPagedAsync(
            tenantId: null,   // tüm tenant'lar
            userId: null,
            entityType: "SuperAdmin",
            action: null,
            pageIndex: req.PageIndex,
            pageSize: req.PageSize,
            ct);

        var dtos = items.Select(l => new AuditLogs.AuditLogDto(
            l.Id, l.UserEmail, l.Action,
            l.EntityType, l.EntityId, l.Details,
            l.IpAddress, l.CreatedAt)).ToList();

        return new Paginate<AuditLogs.AuditLogDto>
        {
            Index = req.PageIndex,
            Size = req.PageSize,
            Count = total,
            Items = dtos
        };
    }
}

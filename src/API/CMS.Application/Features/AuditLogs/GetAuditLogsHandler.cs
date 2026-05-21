using CMS.Application.Common.Abstractions;
using CMS.Domain.Common;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Pagination;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.AuditLogs;

public sealed record GetAuditLogsQuery(
    Guid? UserId = null,
    string? EntityType = null,
    string? Action = null,
    int PageIndex = 0,
    int PageSize = 50)
    : IRequest<Result<Paginate<AuditLogDto>>>;

[RequirePermission("tenants.manage")]
public sealed class GetAuditLogsHandler(
    IAuditLogRepository auditLogRepository,
    ITenantContext tenantContext)
    : IRequestHandler<GetAuditLogsQuery, Result<Paginate<AuditLogDto>>>
{
    public async Task<Result<Paginate<AuditLogDto>>> Handle(
        GetAuditLogsQuery request, CancellationToken ct)
    {
        var (items, total) = await auditLogRepository.GetPagedAsync(
            tenantContext.TenantId,
            request.UserId,
            request.EntityType,
            request.Action,
            request.PageIndex,
            request.PageSize,
            ct);

        var dtos = items.Select(l => new AuditLogDto(
            l.Id, l.UserEmail, l.Action,
            l.EntityType, l.EntityId, l.Details,
            l.IpAddress, l.CreatedAt)).ToList();

        return new Paginate<AuditLogDto>
        {
            Index = request.PageIndex,
            Size = request.PageSize,
            Count = total,
            Items = dtos
        };
    }
}

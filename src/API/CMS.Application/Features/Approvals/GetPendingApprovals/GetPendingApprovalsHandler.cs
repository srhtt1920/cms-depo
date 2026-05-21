using CMS.Application.Common.Abstractions;
using CMS.Domain.Contents;
using CMS.Domain.Identity;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Pagination;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Approvals.GetPendingApprovals;

public sealed record GetPendingApprovalsQuery(
    string? Status = "PendingApproval",
    int PageIndex = 0,
    int PageSize = 20)
    : IRequest<Result<Paginate<ApprovalItemDto>>>;

[RequirePermission("content.approve")]
public sealed class GetPendingApprovalsHandler(
    IContentRepository contentRepo,
    IUserRepository userRepo,
    ITenantContext tenantContext)
    : IRequestHandler<GetPendingApprovalsQuery, Result<Paginate<ApprovalItemDto>>>
{
    public async Task<Result<Paginate<ApprovalItemDto>>> Handle(
        GetPendingApprovalsQuery req, CancellationToken ct)
    {
        var statusFilter = req.Status == "PendingApproval"
            ? ContentStatus.PendingApproval
            : (ContentStatus?)null;

        var page = await contentRepo.GetListAsync(
            search: null,
            status: statusFilter,
            contentType: null,
            pageIndex: req.PageIndex,
            pageSize: req.PageSize,
            ct);

        var userIds = page.Items
            .Where(c => c.Approval?.SubmittedByUserId is not null)
            .Select(c => c.Approval!.SubmittedByUserId!)
            .Distinct().ToList();

        // Kullanıcı e-postalarını toplu al
        var users = new Dictionary<Domain.Identity.UserId, string>();
        foreach (var uid in userIds)
        {
            var u = await userRepo.GetByIdAsync(uid, ct);
            if (u is not null) users[uid] = u.Email;
        }

        var dtos = page.Items.Select(c =>
        {
            var submittedBy = c.Approval?.SubmittedByUserId;
            var email = submittedBy is not null && users.TryGetValue(submittedBy, out var e) ? e : "—";
            var title = c.Translations.FirstOrDefault()?.Title ?? c.Slug.Value;
            return new ApprovalItemDto(
                c.Id.Value, c.Slug.Value, title,
                c.Status.ToString(),
                submittedBy?.Value ?? Guid.Empty,
                email,
                c.Approval?.SubmittedAt ?? c.CreatedAt,
                c.Approval?.Note);
        }).ToList();

        return new Paginate<ApprovalItemDto>
        {
            Index = req.PageIndex,
            Size = req.PageSize,
            Count = page.Count,
            Items = dtos
        };
    }
}

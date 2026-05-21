using CMS.Application.Common.Abstractions;
using CMS.Domain.Contents;
using CMS.Domain.Identity;
using CMS.Domain.Tenants;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Approvals.GetApprovalDetail;

public sealed record GetApprovalDetailQuery(Guid ContentId)
    : IRequest<Result<ApprovalDetailDto>>;

[RequirePermission("content.approve")]
public sealed class GetApprovalDetailHandler(
    IContentRepository contentRepo,
    IUserRepository userRepo,
    ITenantContext tenantContext)
    : IRequestHandler<GetApprovalDetailQuery, Result<ApprovalDetailDto>>
{
    public async Task<Result<ApprovalDetailDto>> Handle(
        GetApprovalDetailQuery req, CancellationToken ct)
    {
        var content = await contentRepo.GetByIdWithSectionsAsync(ContentId.From(req.ContentId), ct);
        if (content is null)
            return Result.Failure<ApprovalDetailDto>(Error.NotFound("Content.NotFound", "Content not found."));
        if (content.TenantId != TenantId.From(tenantContext.TenantId))
            return Result.Failure<ApprovalDetailDto>(Error.Forbidden("Content.WrongTenant", "Access denied."));

        var submitter = content.Approval?.SubmittedByUserId is not null
            ? await userRepo.GetByIdAsync(content.Approval.SubmittedByUserId, ct)
            : null;
        var reviewer = content.Approval?.ReviewedByUserId is not null
            ? await userRepo.GetByIdAsync(content.Approval.ReviewedByUserId, ct)
            : null;

        var translations = content.Translations.Select(t =>
            new ApprovalTranslationDto(t.LanguageCode, t.Title, t.Body)).ToList();

        return new ApprovalDetailDto(
            content.Id.Value, content.Slug.Value,
            content.ContentType.ToString(),
            translations,
            content.Status.ToString(),
            content.Approval?.SubmittedByUserId?.Value ?? Guid.Empty,
            submitter?.Email ?? "—",
            content.Approval?.SubmittedAt ?? content.CreatedAt,
            content.Approval?.Note,
            content.Approval?.ReviewedByUserId?.Value,
            reviewer?.Email,
            content.Approval?.ReviewedAt,
            content.Approval?.ReviewComment);
    }
}

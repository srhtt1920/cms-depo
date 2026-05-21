using CMS.Application.Common.Abstractions;
using CMS.Domain.Contents;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Approvals.ApproveContent;

public sealed record ApproveContentCommand(Guid ContentId, string? Comment) : IRequest<Result>;

[RequirePermission("content.approve")]
public sealed class ApproveContentHandler(
    IContentRepository contentRepo,
    ICurrentUser currentUser,
    IAuditLogger auditLogger)
    : IRequestHandler<ApproveContentCommand, Result>
{
    public async Task<Result> Handle(ApproveContentCommand req, CancellationToken ct)
    {
        var content = await contentRepo.GetByIdAsync(ContentId.From(req.ContentId), ct);
        if (content is null)
            return Result.Failure(Error.NotFound("Content.NotFound", "Content not found."));

        try
        {
            content.Approve(Domain.Identity.UserId.From(currentUser.UserId), req.Comment);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Validation("Approval.InvalidState", ex.Message));
        }

        await contentRepo.UpdateAsync(content, ct);
        await auditLogger.LogAsync("Content.Approved", "Content",
            req.ContentId.ToString(), req.Comment, ct);
        return Result.Success();
    }
}


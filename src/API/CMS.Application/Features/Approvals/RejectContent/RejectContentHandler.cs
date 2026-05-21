using CMS.Application.Common.Abstractions;
using CMS.Domain.Contents;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Approvals.RejectContent;

public sealed record RejectContentCommand(Guid ContentId, string Reason) : IRequest<Result>;

[RequirePermission("content.approve")]
public sealed class RejectContentHandler(
    IContentRepository contentRepo,
    ICurrentUser currentUser,
    IAuditLogger auditLogger)
    : IRequestHandler<RejectContentCommand, Result>
{
    public async Task<Result> Handle(RejectContentCommand req, CancellationToken ct)
    {
        var content = await contentRepo.GetByIdAsync(ContentId.From(req.ContentId), ct);
        if (content is null)
            return Result.Failure(Error.NotFound("Content.NotFound", "Content not found."));

        try
        {
            content.Reject(Domain.Identity.UserId.From(currentUser.UserId), req.Reason);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Validation("Approval.InvalidState", ex.Message));
        }

        await contentRepo.UpdateAsync(content, ct);
        await auditLogger.LogAsync("Content.Rejected", "Content",
            req.ContentId.ToString(), req.Reason, ct);
        return Result.Success();
    }
}


using CMS.Application.Common.Abstractions;
using CMS.Domain.Contents;
using CMS.Domain.Identity;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.Lock.SetContentLock;

public sealed record SetContentLockCommand(
    Guid ContentId,
    bool IsLocked,
    string LockType = "Soft",
    string? LockReason = null,
    bool RequiresSuperAdmin = false
    ) : IRequest<Result>;

[RequirePermission("content.manage")]
public sealed class SetContentLockHandler(
    IContentLockRepository lockRepo,
    ICurrentUser currentUser)
    : IRequestHandler<SetContentLockCommand, Result>
{
    public async Task<Result> Handle(SetContentLockCommand req, CancellationToken ct)
    {
        var contentId = ContentId.From(req.ContentId);
        var userId = UserId.From(currentUser.UserId);

        if (!req.IsLocked)
        {
            await lockRepo.DeleteByContentIdAsync(contentId, ct);
            return Result.Success();
        }

        var lockEntry = ContentLock.ForContent(contentId, req.LockType, userId, req.LockReason, req.RequiresSuperAdmin);
        await lockRepo.UpsertAsync(lockEntry, ct);
        return Result.Success();
    }
}

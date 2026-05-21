using CMS.Application.Common.Abstractions;
using CMS.Domain.Contents;
using CMS.Domain.Identity;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.Lock.BulkLockContents;

[RequirePermission("content.manage")]
public sealed class BulkLockContentsHandler(
    IContentLockRepository lockRepo,
    ICurrentUser currentUser)
    : IRequestHandler<BulkLockContentsCommand, Result>
{
    public async Task<Result> Handle(BulkLockContentsCommand req, CancellationToken ct)
    {
        var contentIds = req.ContentIds.Select(ContentId.From).ToList();
        var userId = UserId.From(currentUser.UserId);

        if (!req.IsLocked)
        {
            foreach (var id in contentIds)
                await lockRepo.DeleteByContentIdAsync(id, ct);
            return Result.Success();
        }

        var locks = contentIds.Select(id => ContentLock.ForContent(id, req.LockType, userId));
        await lockRepo.BulkUpsertAsync(locks, ct);
        return Result.Success();
    }
}

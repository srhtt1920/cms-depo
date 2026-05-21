using CMS.Application.Common.Abstractions;
using CMS.Domain.Contents;
using CMS.Domain.Identity;
using CMS.Domain.Pages;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.Lock.SetPageLock;

[RequirePermission("pages.manage")]
public sealed class SetPageLockHandler(
  IContentLockRepository lockRepo,
  ICurrentUser currentUser)
  : IRequestHandler<SetPageLockCommand, Result>
{
    public async Task<Result> Handle(SetPageLockCommand req, CancellationToken ct)
    {
        var pageId = PageId.From(req.PageId);
        var userId = UserId.From(currentUser.UserId);

        if (!req.IsLocked)
        {
            await lockRepo.DeleteByPageIdAsync(pageId, ct);
            return Result.Success();
        }

        var lockEntry = ContentLock.ForPage(pageId, req.LockType, userId);
        await lockRepo.UpsertAsync(lockEntry, ct);
        return Result.Success();
    }
}

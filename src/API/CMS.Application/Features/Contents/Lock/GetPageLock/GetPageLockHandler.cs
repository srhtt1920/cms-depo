using CMS.Domain.Contents;
using CMS.Domain.Identity;
using CMS.Domain.Pages;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.Lock.GetPageLock;

[RequirePermission("pages.manage")]
public sealed class GetPageLockHandler(
 IContentLockRepository lockRepo,
 IUserRepository userRepo)
 : IRequestHandler<GetPageLockQuery, Result<ContentLockDto>>
{
    public async Task<Result<ContentLockDto>> Handle(GetPageLockQuery req, CancellationToken ct)
    {
        var pageId = PageId.From(req.PageId);
        var lockEntry = await lockRepo.GetByPageIdAsync(pageId, ct);

        if (lockEntry is null)
            return new ContentLockDto(req.PageId, "Page", false, null, null, null, null);

        var user = await userRepo.GetByIdAsync(lockEntry.LockedByUserId, ct);
        return new ContentLockDto(
            req.PageId, "Page", true,
            lockEntry.LockType,
            lockEntry.LockedByUserId.Value,
            user?.Email,
            lockEntry.LockedAt,
            lockEntry.LockReason,
            lockEntry.RequiresSuperAdmin);
    }
}
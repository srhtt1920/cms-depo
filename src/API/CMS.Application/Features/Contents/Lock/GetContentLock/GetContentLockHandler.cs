using CMS.Application.Common.Abstractions;
using CMS.Domain.Contents;
using CMS.Domain.Identity;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.Lock.GetContentLock;

[RequirePermission("content.read")]
public sealed class GetContentLockHandler(
    IContentLockRepository lockRepo,
    IUserRepository userRepo,
    ITenantContext tenantContext)
    : IRequestHandler<GetContentLockQuery, Result<ContentLockDto>>
{
    public async Task<Result<ContentLockDto>> Handle(GetContentLockQuery req, CancellationToken ct)
    {
        var contentId = ContentId.From(req.ContentId);
        var lockEntry = await lockRepo.GetByContentIdAsync(contentId, ct);

        if (lockEntry is null)
            return new ContentLockDto(req.ContentId, "Content", false, null, null, null, null);

        var user = await userRepo.GetByIdAsync(lockEntry.LockedByUserId, ct);
        return new ContentLockDto(
            req.ContentId, "Content", true,
            lockEntry.LockType,
            lockEntry.LockedByUserId.Value,
            user?.Email,
            lockEntry.LockedAt,
            lockEntry.LockReason,
            lockEntry.RequiresSuperAdmin);
    }
}
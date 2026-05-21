using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.Lock.SetPageLock;

public sealed record SetPageLockCommand(
     Guid PageId,
     bool IsLocked,
     string LockType = "Soft"
    ) : IRequest<Result>;

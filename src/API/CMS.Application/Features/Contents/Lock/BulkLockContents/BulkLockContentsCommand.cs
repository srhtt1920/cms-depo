using CMS.SharedKernel.Result;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Application.Features.Contents.Lock.BulkLockContents;

public sealed record BulkLockContentsCommand(
    List<Guid> ContentIds,
    bool IsLocked,
    string LockType = "Soft"
) : IRequest<Result>;

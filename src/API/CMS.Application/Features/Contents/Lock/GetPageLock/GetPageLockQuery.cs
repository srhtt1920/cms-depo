using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.Lock.GetPageLock;

public sealed record GetPageLockQuery(Guid PageId) : IRequest<Result<ContentLockDto>>;

using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.Lock.GetContentLock;

public sealed record GetContentLockQuery(Guid ContentId) : IRequest<Result<ContentLockDto>>;

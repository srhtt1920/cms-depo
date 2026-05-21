using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.Completion;
public sealed record GetCompletionQuery(Guid ContentId) : IRequest<Result<ContentCompletionDto>>;

using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.PublishContent;

public sealed record PublishContentCommand(Guid ContentId) : IRequest<Result>;

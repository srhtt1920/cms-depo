using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.UnpublishContent;

public sealed record UnpublishContentCommand(Guid ContentId) : IRequest<Result>;


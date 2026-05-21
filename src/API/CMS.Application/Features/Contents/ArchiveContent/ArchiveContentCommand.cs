using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.ArchiveContent;

public sealed record ArchiveContentCommand(Guid ContentId) : IRequest<Result>;

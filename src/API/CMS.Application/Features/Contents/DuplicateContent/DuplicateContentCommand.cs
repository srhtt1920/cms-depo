using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.DuplicateContent;

public sealed record DuplicateContentCommand(Guid ContentId, string NewSlug)
 : IRequest<Result<DuplicateContentResponse>>;

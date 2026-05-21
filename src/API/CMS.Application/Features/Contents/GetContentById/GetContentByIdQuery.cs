using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.GetContentById;

public sealed record GetContentByIdQuery(Guid Id) : IRequest<Result<ContentDetailDto>>;

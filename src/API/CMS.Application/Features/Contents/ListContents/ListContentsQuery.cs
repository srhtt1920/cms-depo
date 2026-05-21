using CMS.Domain.Contents;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.ListContents;

public sealed record ListContentsQuery(
    string?        Search    = null,
    ContentStatus? Status    = null,
    ContentType? ContentType = null,
    int PageIndex = 0,
    int            PageSize  = 20
) : IRequest<Result<ListContentsResponse>>;

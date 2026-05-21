using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.GetContent;

public sealed record GetContentQuery(string Slug) : IRequest<Result<ContentDto>>;

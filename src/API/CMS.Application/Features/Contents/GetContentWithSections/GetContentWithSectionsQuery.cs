using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.GetContentWithSections;

public sealed record GetContentWithSectionsQuery(Guid ContentId)
    : IRequest<Result<ContentWithSectionsDto>>;

using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.GetVersions;


public sealed record GetVersionsQuery(Guid ContentId) : IRequest<Result<List<ContentVersionDto>>>;

public sealed record ContentVersionDto(
    Guid Id,
    int VersionNumber,
    string? Label,
    bool IsPublished,
    Guid CreatedBy,
    DateTime CreatedAt);

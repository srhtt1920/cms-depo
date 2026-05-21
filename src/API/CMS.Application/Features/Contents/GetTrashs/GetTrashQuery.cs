using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.GetTrashs;

public sealed record GetTrashQuery : IRequest<Result<List<TrashItemDto>>>;

public sealed record TrashItemDto(
    Guid Id,
    string Slug,
    string Title,
    List<string> Languages,
    DateTime? DeletedAt);

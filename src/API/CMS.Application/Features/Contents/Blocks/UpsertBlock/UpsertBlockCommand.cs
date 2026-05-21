using CMS.Domain.Contents;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.Blocks.UpsertBlock;

public sealed record UpsertBlockCommand(
    Guid      ContentId,
    Guid      SectionId,
    Guid?     BlockId,       // null = create, value = update
    BlockType BlockType,
    int       Order,
    bool      IsVisible,
    string    Settings       // JSON
) : IRequest<Result<Guid>>;

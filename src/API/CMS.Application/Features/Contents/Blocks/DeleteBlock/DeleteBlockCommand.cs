using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.Blocks.DeleteBlock;

public sealed record DeleteBlockCommand(Guid ContentId, Guid SectionId, Guid BlockId) 
    : IRequest<Result>;

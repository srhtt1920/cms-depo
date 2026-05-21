using CMS.Domain.Contents;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.Sections.CreateSection;

public sealed record CreateSectionCommand(
    Guid    ContentId,
    string  Name,
    int     Order,
    Guid?   ParentSectionId = null,
    string? CssClass        = null,
    string? AnimationType   = null,
    int     AnimationDuration = 300,
    int     AnimationDelay    = 0,
    string  AnimationEasing   = "ease-out"
) : IRequest<Result<Guid>>;

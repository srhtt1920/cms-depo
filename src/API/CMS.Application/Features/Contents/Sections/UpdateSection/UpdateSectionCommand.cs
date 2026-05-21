using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.Sections.UpdateSection;

public sealed record UpdateSectionCommand(
    Guid    ContentId,
    Guid    SectionId,
    string  Name,
    int     Order,
    bool    IsVisible,
    bool    IsEnabled,
    string? CssClass,
    string? AnimationType,
    int     AnimationDuration = 300,
    int     AnimationDelay    = 0,
    string  AnimationEasing   = "ease-out"
) : IRequest<Result>;

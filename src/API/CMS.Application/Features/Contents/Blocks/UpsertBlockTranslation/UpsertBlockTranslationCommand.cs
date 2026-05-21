using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.Blocks.UpsertBlockTranslation;

public sealed record UpsertBlockTranslationCommand(
    Guid    ContentId,
    Guid    SectionId,
    Guid    BlockId,
    string  LanguageCode,
    string? Title,
    string? Body,
    string? AltText,
    string? LinkText
) : IRequest<Result>;

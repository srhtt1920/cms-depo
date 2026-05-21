using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.UpdateContent;

public sealed record UpdateContentCommand(
    Guid ContentId,
    string LanguageCode,
    string Title,
    string Body,
    string? MetaTitle,
    string? MetaDescription) : IRequest<Result>;

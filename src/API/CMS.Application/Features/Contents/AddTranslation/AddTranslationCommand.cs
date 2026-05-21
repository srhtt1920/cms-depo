using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.AddTranslation;

public sealed record AddTranslationCommand(
    Guid ContentId,
    string LanguageCode,
    string Title,
    string Body,
    string? MetaTitle,
    string? MetaDescription) : IRequest<Result>;

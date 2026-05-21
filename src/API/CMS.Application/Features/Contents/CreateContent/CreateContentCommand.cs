using CMS.Domain.Contents;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.CreateContent;

public sealed record CreateContentCommand(
    string Slug,
    string LanguageCode,
    string Title,
    string Body,
    string? MetaTitle,
    string? MetaDescription,
    ContentType ContentType = ContentType.Page,
    Guid? ParentId = null): IRequest<Result<CreateContentResponse>>;

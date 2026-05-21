using CMS.Application.Common.Abstractions;
using CMS.Domain.Contents;
using CMS.Domain.Contents.Errors;
using CMS.Domain.Tenants;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.CreateContent;

[RequirePermission("content.create")]
public sealed class CreateContentHandler(
    IContentRepository contentRepository,
    ITenantContext tenantContext)
    : IRequestHandler<CreateContentCommand, Result<CreateContentResponse>>
{
    public async Task<Result<CreateContentResponse>> Handle(
        CreateContentCommand request, CancellationToken ct)
    {
        var slugResult = Slug.TryCreate(request.Slug);
        if (slugResult.IsFailure)
            return Result.Failure<CreateContentResponse>(slugResult.Error);

        var slugExists = await contentRepository.SlugExistsAsync(
            request.Slug, tenantContext.TenantId, ct: ct);
        
        if (slugExists)
            return Result.Failure<CreateContentResponse>(
                ContentErrors.SlugAlreadyExists(request.Slug));

        var content = Content.Create(
            TenantId.From(tenantContext.TenantId),
            request.Slug,
            request.LanguageCode,
            request.Title,
            request.Body,
            request.MetaTitle,
            request.MetaDescription,
            request.ContentType);

        await contentRepository.AddAsync(content, ct);
        return new CreateContentResponse(content.Id.Value, content.Slug.Value);
    }
}

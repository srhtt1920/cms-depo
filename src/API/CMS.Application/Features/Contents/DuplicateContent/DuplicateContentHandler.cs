using CMS.Application.Common.Abstractions;
using CMS.Domain.Contents;
using CMS.Domain.Tenants;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.DuplicateContent;


[RequirePermission("content.create")]
public sealed class DuplicateContentHandler(
    IContentRepository contentRepository,
    ITenantContext tenantContext)
    : IRequestHandler<DuplicateContentCommand, Result<DuplicateContentResponse>>
{
    public async Task<Result<DuplicateContentResponse>> Handle(
        DuplicateContentCommand request, CancellationToken ct)
    {
        var source = await contentRepository.GetByIdWithSectionsAsync(
            ContentId.From(request.ContentId), ct);

        if (source is null)
            return Result.Failure<DuplicateContentResponse>(
                Error.NotFound("Content.NotFound", "Source content not found."));

        var slugResult = Slug.TryCreate(request.NewSlug);
        if (slugResult.IsFailure)
            return Result.Failure<DuplicateContentResponse>(slugResult.Error);

        var slugExists = await contentRepository.SlugExistsAsync(
            request.NewSlug, tenantContext.TenantId, ct:ct);
        if (slugExists)
            return Result.Failure<DuplicateContentResponse>(
                Error.Conflict("Content.SlugExists", $"Slug '{request.NewSlug}' already in use."));

        var tenantId = TenantId.From(tenantContext.TenantId);

        // İlk çeviriyi al
        var firstTrans = source.Translations.FirstOrDefault();
        if (firstTrans is null)
            return Result.Failure<DuplicateContentResponse>(
                Error.Validation("Content.NoTranslation", "Source content has no translations."));

        var copy = Content.Create(
            tenantId,
            request.NewSlug,
            firstTrans.LanguageCode,
            $"[Kopya] {firstTrans.Title}",
            firstTrans.Body,
            firstTrans.MetaTitle,
            firstTrans.MetaDescription,
            source.ContentType,
            source.AuthorId);

        // Diğer çevirileri ekle
        foreach (var t in source.Translations.Skip(1))
            copy.AddTranslation(t.LanguageCode, t.Title, t.Body, t.MetaTitle, t.MetaDescription);

        await contentRepository.AddAsync(copy, ct);
        return new DuplicateContentResponse(copy.Id.Value, copy.Slug.Value);
    }
}

using CMS.Application.Common.Abstractions;
using CMS.Domain.Contents;
using CMS.Domain.Contents.Errors;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.GetContent;

public sealed class GetContentHandler(
    IContentRepository contentRepository,
    ITenantContext tenantContext,
    ILanguageContext languageContext)
    : IRequestHandler<GetContentQuery, Result<ContentDto>>
{
    public async Task<Result<ContentDto>> Handle(
        GetContentQuery request,
        CancellationToken ct)
    {
        var content = await contentRepository.GetBySlugAsync(
            request.Slug, tenantContext.TenantId, ct);
        if (content is null)
            return Result.Failure<ContentDto>(ContentErrors.NotFound(request.Slug));

        var translation = await contentRepository.GetTranslationAsync(
            content.Id,
            languageContext.LanguageCandidates,
            ct);
        if (translation is null)
            return Result.Failure<ContentDto>(
                ContentErrors.TranslationNotFound(languageContext.RequestedLanguage));

        // PublishedAt: translation publish tarihi (UpdatedAt yoksa CreatedAt)
        DateTime? publishedAt = translation.IsPublished
            ? (translation.UpdatedAt ?? translation.CreatedAt)
            : null;

        return new ContentDto(
            content.Id.Value,
            content.Slug.Value,
            translation.LanguageCode,
            translation.Title,
            translation.Body,
            translation.MetaTitle,
            translation.MetaDescription,
            content.Status.ToString(),
            publishedAt);
    }
}

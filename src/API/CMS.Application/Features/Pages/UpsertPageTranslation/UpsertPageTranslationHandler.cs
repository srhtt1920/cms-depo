using CMS.Application.Common.Abstractions;
using CMS.Domain.Pages;
using CMS.Domain.Pages.Errors;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Pages.UpsertPageTranslation;
public sealed record UpsertPageTranslationCommand(
    Guid PageId,
    string LanguageCode,
    string Title,
    string LinkName,
    string Slug,
    string? MetaTitle,
    string? MetaDescription,
    string? MetaKeywords
    ) : IRequest<Result>;

[RequirePermission("page.update")]
public sealed class UpsertPageTranslationHandler(
    IPageRepository pageRepository,
    ITenantContext tenantContext)
    : IRequestHandler<UpsertPageTranslationCommand, Result>
{
    public async Task<Result> Handle(
        UpsertPageTranslationCommand request, CancellationToken ct)
    {
        var page = await pageRepository.GetByIdAsync(PageId.From(request.PageId), ct);

        if (page is null || page.TenantId.Value != tenantContext.TenantId)
            return Result.Failure(PageErrors.NotFound(request.PageId));

        // Slug çakışma kontrolü — mevcut sayfayı hariç tut
        var slugExists = await pageRepository.SlugExistsAsync(
            request.Slug, request.LanguageCode, tenantContext.TenantId,
            excludePageId: page.Id, ct);

        if (slugExists)
            return Result.Failure(PageErrors.SlugAlreadyExists(request.Slug, request.LanguageCode));

        var existingTranslation = page.Translations
            .FirstOrDefault(t => t.LanguageCode == request.LanguageCode.ToLowerInvariant());

        if (existingTranslation is null)
            page.AddTranslation(
                request.LanguageCode, request.Title, request.LinkName, request.Slug,
                request.MetaTitle, request.MetaDescription, request.MetaKeywords);
        else
            page.UpdateTranslation(
                request.LanguageCode, request.Title, request.LinkName, request.Slug,
                request.MetaTitle, request.MetaDescription, request.MetaKeywords);

        await pageRepository.UpdateAsync(page, ct);
        return Result.Success();
    }
}

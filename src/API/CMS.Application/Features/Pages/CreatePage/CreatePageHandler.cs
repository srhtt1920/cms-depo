using CMS.Application.Common.Abstractions;
using CMS.Domain.Contents;
using CMS.Domain.Pages;
using CMS.Domain.Pages.Errors;
using CMS.Domain.Tenants;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Pages.CreatePage;

public sealed record CreatePageCommand(
    PageType PageType,
    ContentType? ContentType,
    Guid? ParentId,
    int Order,

    // --- İlk çeviri ---
    string LanguageCode,
    string Title,
    string LinkName,
    string Slug,
    string? MetaTitle,
    string? MetaDescription,
    string? MetaKeywords,

    // --- Opsiyonel ---
    Guid? LinkedContentId,
    string? Icon,
    string? ExternalUrl
) : IRequest<Result<CreatePageResponse>>;

[RequirePermission("page.create")]
public sealed class CreatePageHandler(
    IPageRepository pageRepository,
    ITenantContext tenantContext)
    : IRequestHandler<CreatePageCommand, Result<CreatePageResponse>>
{
    public async Task<Result<CreatePageResponse>> Handle(
        CreatePageCommand request, CancellationToken ct)
    {
        var tenantId = TenantId.From(tenantContext.TenantId);

        // 1. Parent var mı?
        if (request.ParentId.HasValue)
        {
            var parent = await pageRepository.GetByIdAsync(
                PageId.From(request.ParentId.Value), ct);

            if (parent is null)
                return Result.Failure<CreatePageResponse>(
                    PageErrors.ParentNotFound(request.ParentId.Value));
        }

        // 2. Slug benzersiz mi?
        var slugExists = await pageRepository.SlugExistsAsync(
            request.Slug, request.LanguageCode, tenantContext.TenantId, null, ct);

        if (slugExists)
            return Result.Failure<CreatePageResponse>(
                PageErrors.SlugAlreadyExists(request.Slug, request.LanguageCode));

        // 3. Page oluştur
        var parentId = request.ParentId.HasValue
            ? PageId.From(request.ParentId.Value)
            : null;

        Page page;

        if (request.PageType == PageType.Category)
        {
            page = Page.CreateCategory(
                tenantId,
                request.LanguageCode,
                request.Title,
                request.LinkName,
                request.Slug,
                parentId,
                request.Order,
                request.Icon);
        }
        else
        {
            var linkedContentId = request.LinkedContentId.HasValue
                ? ContentId.From(request.LinkedContentId.Value)
                : null;

            page = Page.CreateNormal(
                tenantId,
                request.ContentType!.Value,
                request.LanguageCode,
                request.Title,
                request.LinkName,
                request.Slug,
                parentId,
                request.Order,
                linkedContentId,
                request.Icon,
                request.ExternalUrl);
        }

        // Meta bilgileri ilk çeviriye aktar
        if (request.MetaTitle != null || request.MetaDescription != null || request.MetaKeywords != null)
        {
            page.UpdateTranslation(
                request.LanguageCode,
                request.Title,
                request.LinkName,
                request.Slug,
                request.MetaTitle,
                request.MetaDescription,
                request.MetaKeywords);
        }

        await pageRepository.AddAsync(page, ct);

        return new CreatePageResponse(
            page.Id.Value,
            page.Translations.First().Slug,
            page.PageType.ToString());
    }
}
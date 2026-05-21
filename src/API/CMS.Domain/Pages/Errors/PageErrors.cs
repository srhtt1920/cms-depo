using CMS.SharedKernel.Result;

namespace CMS.Domain.Pages.Errors;

public static class PageErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("Page.NotFound", $"Page '{id}' was not found.");

    public static Error ParentNotFound(Guid parentId) =>
        Error.NotFound("Page.ParentNotFound", $"Parent page '{parentId}' was not found.");

    public static Error SlugAlreadyExists(string slug, string languageCode) =>
        Error.Conflict("Page.SlugExists",
            $"Slug '{slug}' already exists for language '{languageCode}' in this tenant.");

    public static Error ContentTypeRequiredForNormalPage =>
        Error.Validation("Page.ContentTypeRequired",
            "ContentType is required for Normal page type.");

    public static Error CategoryCannotHaveContentType =>
        Error.Validation("Page.CategoryContentType",
            "Category pages cannot have a ContentType.");

    public static Error CircularReference =>
        Error.Validation("Page.CircularReference",
            "A page cannot be moved under one of its own descendants.");

    public static Error TranslationNotFound(string languageCode) =>
        Error.NotFound("Page.TranslationNotFound",
            $"No translation found for language '{languageCode}'.");

    public static Error TranslationAlreadyExists(string languageCode) =>
        Error.Conflict("Page.TranslationExists",
            $"Translation for '{languageCode}' already exists.");

    public static Error CannotLinkContentToCategory =>
        Error.Validation("Page.CannotLinkToCategory",
            "Category pages cannot be linked to a content item.");
}
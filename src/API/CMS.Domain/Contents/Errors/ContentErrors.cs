using CMS.SharedKernel.Result;

namespace CMS.Domain.Contents.Errors;

public static class ContentErrors
{
    public static Error NotFound(string slug) =>
        Error.NotFound("Content.NotFound", $"Content with slug '{slug}' was not found.");

    public static Error NotFoundById(Guid id) =>
        Error.NotFound("Content.NotFound", $"Content with id '{id}' was not found.");

    public static Error TranslationNotFound(string languageCode) =>
        Error.NotFound("Content.TranslationNotFound", $"No translation available for language '{languageCode}'.");

    public static Error SlugAlreadyExists(string slug) =>
        Error.Conflict("Content.SlugExists", $"Slug '{slug}' already exists for this tenant.");

    public static Error CannotPublishArchived =>
        Error.Conflict("Content.CannotPublishArchived", "Archived content cannot be published.");

    public static Error ContentTypeMismatch(string expected, string actual) =>
        Error.Validation("Content.TypeMismatch",
            $"Expected content type '{expected}' but got '{actual}'.");

    public static Error TenantMismatch(Guid contentId) =>
        Error.Forbidden("Content.TenantMismatch",
            $"Content '{contentId}' does not belong to the current tenant.");
}

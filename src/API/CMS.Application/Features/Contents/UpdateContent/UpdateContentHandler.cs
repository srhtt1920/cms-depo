using CMS.Domain.Contents;
using CMS.Domain.Contents.Errors;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.UpdateContent;

[RequirePermission("content.edit")]
public sealed class UpdateContentHandler(IContentRepository contentRepository)
    : IRequestHandler<UpdateContentCommand, Result>
{
    public async Task<Result> Handle(UpdateContentCommand request, CancellationToken ct)
    {
        var content = await contentRepository.GetByIdAsync(
            ContentId.From(request.ContentId), ct);

        if (content is null)
            return Result.Failure(ContentErrors.NotFoundById(request.ContentId));

        var translation = content.Translations
            .FirstOrDefault(t => t.LanguageCode == request.LanguageCode.ToLowerInvariant());

        if (translation is null)
            return Result.Failure(ContentErrors.TranslationNotFound(request.LanguageCode));

        translation.Update(request.Title, request.Body, request.MetaTitle, request.MetaDescription);
        await contentRepository.UpdateAsync(content, ct);
        return Result.Success();
    }
}

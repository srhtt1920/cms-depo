using CMS.Domain.Contents;
using CMS.Domain.Contents.Errors;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.AddTranslation;

[RequirePermission("content.create")]
public sealed class AddTranslationHandler(IContentRepository contentRepository)
    : IRequestHandler<AddTranslationCommand, Result>
{
    public async Task<Result> Handle(AddTranslationCommand request, CancellationToken ct)
    {
        var content = await contentRepository.GetByIdAsync(
            ContentId.From(request.ContentId), ct);

        if (content is null)
            return Result.Failure(ContentErrors.NotFoundById(request.ContentId));

        try { content.AddTranslation(request.LanguageCode, request.Title, request.Body, request.MetaTitle, request.MetaDescription); }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Conflict("Translation.AlreadyExists", ex.Message));
        }

        await contentRepository.UpdateAsync(content, ct);
        return Result.Success();
    }
}

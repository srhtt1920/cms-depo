using CMS.Domain.Contents;
using CMS.Domain.Contents.Errors;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.PublishContent;

[RequirePermission("content.publish")]
public sealed class PublishContentHandler(IContentRepository contentRepository)
    : IRequestHandler<PublishContentCommand, Result>
{
    public async Task<Result> Handle(PublishContentCommand request, CancellationToken ct)
    {
        var content = await contentRepository.GetByIdAsync(
            ContentId.From(request.ContentId), ct);
        if (content is null)
            return Result.Failure(ContentErrors.NotFoundById(request.ContentId));

        if (content.Status == ContentStatus.Archived)
            return Result.Failure(ContentErrors.CannotPublishArchived);

        if (!content.Translations.Any())
            return Result.Failure(Error.Validation(
                "Content.NoTranslations",
                "Content must have at least one translation before publishing."));

        // Tüm mevcut çevirileri yayınla (Content.Publish() zaten bunu yapıyor)
        content.Publish();

        await contentRepository.UpdateAsync(content, ct);
        return Result.Success();
    }
}

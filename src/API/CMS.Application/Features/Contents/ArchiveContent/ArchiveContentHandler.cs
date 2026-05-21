using CMS.Domain.Contents;
using CMS.Domain.Contents.Errors;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.ArchiveContent;

[RequirePermission("content.archive")]
public sealed class ArchiveContentHandler(IContentRepository contentRepository)
    : IRequestHandler<ArchiveContentCommand, Result>
{
    public async Task<Result> Handle(ArchiveContentCommand request, CancellationToken ct)
    {
        var content = await contentRepository.GetByIdAsync(
            ContentId.From(request.ContentId), ct);

        if (content is null)
            return Result.Failure(ContentErrors.NotFoundById(request.ContentId));

        content.Archive();
        await contentRepository.UpdateAsync(content, ct);
        return Result.Success();
    }
}

using CMS.Application.Common.Abstractions;
using CMS.Domain.Contents;
using CMS.Domain.Contents.Errors;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.GetContentById;

[RequirePermission("content.read")]
public sealed class GetContentByIdHandler(
  IContentRepository contentRepository,
  ITenantContext tenantContext)
  : IRequestHandler<GetContentByIdQuery, Result<ContentDetailDto>>
{
    public async Task<Result<ContentDetailDto>> Handle(
        GetContentByIdQuery request, CancellationToken ct)
    {
        var content = await contentRepository.GetByIdAsync(ContentId.From(request.Id), ct);

        // Tenant doğrulaması — başka tenant'ın içeriğine erişilemesin
        if (content is null || content.TenantId.Value != tenantContext.TenantId)
            return Result.Failure<ContentDetailDto>(ContentErrors.NotFoundById(request.Id));

        return new ContentDetailDto(
            content.Id.Value,
            content.Slug.Value,
            content.Status.ToString(),
            content.ContentType.ToString(),
            content.AuthorId?.Value,
            content.CreatedAt,
            content.UpdatedAt,
            content.Translations.Select(t => new ContentTranslationDto(
                t.LanguageCode, t.Title, t.Body,
                t.MetaTitle, t.MetaDescription, t.IsPublished)).ToList(),
            content.Schedule is null ? null : new ContentScheduleDto(
                content.Schedule.PublishAt,
                content.Schedule.UnpublishAt,
                content.Schedule.DurationMinutes,
                content.Schedule.TimeZoneId));
    }
}

using CMS.Application.Common.Abstractions;
using CMS.Domain.Contents;
using CMS.Domain.Tenants;
using CMS.SharedKernel.Attributes;
using CMS.SharedKernel.Result;
using MediatR;

namespace CMS.Application.Features.Contents.GetContentWithSections;

[RequirePermission("content.detailRead")]
public sealed class GetContentWithSectionsHandler(
    IContentRepository contentRepository,
    ITenantContext tenantContext)
    : IRequestHandler<GetContentWithSectionsQuery, Result<ContentWithSectionsDto>>
{
    public async Task<Result<ContentWithSectionsDto>> Handle(
        GetContentWithSectionsQuery request, CancellationToken ct)
    {
        var content = await contentRepository
            .GetByIdWithSectionsAsync(ContentId.From(request.ContentId), ct);

        if (content is null)
            return Result.Failure<ContentWithSectionsDto>(
                Error.NotFound("Content.NotFound", $"Content '{request.ContentId}' not found."));

        if (content.TenantId != TenantId.From(tenantContext.TenantId))
            return Result.Failure<ContentWithSectionsDto>(
                Error.Forbidden("Content.WrongTenant", "Access denied."));

        var scheduleDto = content.Schedule is { } sc
            ? new ScheduleInfoDto(sc.PublishAt, sc.UnpublishAt, sc.DurationMinutes, sc.TimeZoneId)
            : null;

        var sections = content.Sections
            .OrderBy(s => s.Order)
            .Select(s => new SectionDto(
                s.Id.Value,
                s.Name,
                s.Order,
                s.IsVisible,
                s.IsEnabled,
                s.CssClass,
                s.ParentSectionId?.Value,
                s.Animation.Type is null ? null
                    : new AnimationDto(s.Animation.Type, s.Animation.Duration,
                                       s.Animation.Delay, s.Animation.Easing),
                s.Blocks.OrderBy(b => b.Order).Select(b => new BlockDto(
                    b.Id.Value,
                    b.BlockType.ToString(),
                    b.Order,
                    b.IsVisible,
                    b.Settings,
                    b.Translations.Select(t => new BlockTransDto(
                        t.LanguageCode, t.Title, t.Body, t.AltText, t.LinkText))
                    .ToList()))
                .ToList()))
            .ToList();

        return new ContentWithSectionsDto(
            content.Id.Value,
            content.Slug.Value,
            content.Status.ToString(),
            scheduleDto,
            sections);
    }
}

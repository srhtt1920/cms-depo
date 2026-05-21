namespace CMS.Application.Features.Contents.GetContentWithSections;

public sealed record ContentWithSectionsDto(
    Guid Id,
    string Slug,
    string Status,
    ScheduleInfoDto? Schedule,
    List<SectionDto> Sections);

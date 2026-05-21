namespace CMS.Domain.Pages.Dtos;

// ── Breadcrumb DTO ────────────────────────────────────────────────────────────
public sealed record PageBreadcrumbItemDto(
    Guid Id,
    string Title,
    string Slug,
    int Depth);

namespace CMS.Application.Features.Pages.GetPageBreadcrumb;

public sealed class BreadcrumbRawDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int Depth { get; set; }
}

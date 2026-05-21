namespace CMS.SharedKernel.Pagination;

public class PagedRequest
{
    public int PageIndex { get; init; } = 0;
    public int PageSize { get; init; } = 10;
    public DynamicQuery.DynamicQuery? DynamicQuery { get; init; }
}

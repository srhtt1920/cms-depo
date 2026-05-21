namespace CMS.SharedKernel.DynamicQuery;

public sealed class DynamicQuery
{
    public IEnumerable<Sort>? Sort { get; init; }
    public Filter? Filter { get; init; }

    public DynamicQuery() { }
    public DynamicQuery(IEnumerable<Sort>? sort, Filter? filter)
    {
        Sort = sort;
        Filter = filter;
    }
}

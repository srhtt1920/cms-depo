namespace CMS.SharedKernel.DynamicQuery;

public static class DynamicQueryGenerator
{
    public static DynamicQuery GenerateDynamicQuery(
        List<(string Field, string Operator, string Logic, string Value, string GroupKey, string? ParentGroupKey)>? filters,
        Sort sort)
    {
        return new DynamicQuery
        {
            Sort = new List<Sort> { sort },
            Filter = filters is not null ? BuildFilterTree(filters) : null
        };
    }

    private static Filter? BuildFilterTree(
        List<(string Field, string Operator, string Logic, string Value, string GroupKey, string? ParentGroupKey)> filters)
    {
        if (filters.Count == 0) return null;

        var groups = filters
            .GroupBy(f => f.GroupKey)
            .ToDictionary(g => g.Key, g => g.ToList());

        Filter BuildGroup(string groupKey)
        {
            var groupFilters = groups[groupKey];

            var filter = new Filter
            {
                Logic = groupFilters.First().Logic,
                Filters = new List<Filter>()
            };

            var childGroupKeys = groups.Keys
                .Where(k => filters.Any(x => x.GroupKey == k && x.ParentGroupKey == groupKey))
                .ToList();

            foreach (var childKey in childGroupKeys)
                filter.Filters = filter.Filters?.Concat([BuildGroup(childKey)]).ToList();

            var normalFilters = groupFilters
                .Where(f => !childGroupKeys.Contains(f.GroupKey))
                .Select(f => new Filter(f.Field, f.Operator) { Value = f.Value })
                .ToList();

            filter.Filters = filter.Filters?.Concat(normalFilters).ToList();

            if (filter.Filters?.Count() == 1 && string.IsNullOrEmpty(filter.Logic))
                return filter.Filters.First();

            return filter;
        }

        var rootGroupKeys = groups.Keys
            .Where(k => filters.Any(f => f.GroupKey == k && f.ParentGroupKey == null))
            .ToList();

        return rootGroupKeys.Count == 1
            ? BuildGroup(rootGroupKeys[0])
            : new Filter
            {
                Logic = "or",
                Filters = rootGroupKeys.Select(k => BuildGroup(k)).ToList()
            };
    }
}

namespace CMS.SharedKernel.DynamicQuery;

public sealed class Filter
{
    public string Field { get; set; } = string.Empty;
    public string? Value { get; set; }
    public string Operator { get; set; } = string.Empty;
    public string? Logic { get; set; }
    public IEnumerable<Filter>? Filters { get; set; }

    public Filter() { }
    public Filter(string field, string @operator) { Field = field; Operator = @operator; }
}

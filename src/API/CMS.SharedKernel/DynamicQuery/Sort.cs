namespace CMS.SharedKernel.DynamicQuery;

public sealed class Sort
{
    public string Field { get; set; } = string.Empty;
    public string Dir { get; set; } = string.Empty;

    public Sort() { }
    public Sort(string field, string dir) { Field = field; Dir = dir; }
}

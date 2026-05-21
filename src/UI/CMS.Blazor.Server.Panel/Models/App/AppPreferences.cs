namespace CMS.Blazor.Server.Panel.Models.App;

public sealed class AppPreferences
{
    public string Theme { get; set; } = "light";   // light | dark
    public string Language { get; set; } = "tr";      // tr | en | de
    public string MenuTheme { get; set; } = "dark";    // dark | light | colored
    public string SizeMode { get; set; } = "default"; // default | compact | comfortable
}

namespace CMS.Blazor.Server.Panel.Services;

/// <summary>
/// wwwroot/icons/{name}.svg dosyalarını okuyup cache'ler.
/// Blazor bileşenlerinde &lt;SvgIcon Name="home" /&gt; şeklinde kullanılır.
/// </summary>
public sealed class SvgIconService(IWebHostEnvironment env)
{
    private readonly Dictionary<string, string> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _iconDir = Path.Combine(env.WebRootPath, "icons");

    public string Get(string name)
    {
        if (_cache.TryGetValue(name, out var cached)) return cached;

        var path = Path.Combine(_iconDir, $"{name}.svg");
        if (!File.Exists(path)) return string.Empty;

        var svg = File.ReadAllText(path);
        _cache[name] = svg;
        return svg;
    }

    public bool Exists(string name) =>
        File.Exists(Path.Combine(_iconDir, $"{name}.svg"));
}
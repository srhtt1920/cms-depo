using CMS.Application.Common.Abstractions;

namespace CMS.Infrastructure.Identity;

public sealed class LanguageContext : ILanguageContext
{
    public string RequestedLanguage { get; private set; } = "en";
    public string FallbackLanguage { get; private set; } = "en";
    public string DefaultLanguage { get; private set; } = "en";

    public string[] LanguageCandidates =>
        new[] { RequestedLanguage, FallbackLanguage, DefaultLanguage }
            .Distinct()
            .ToArray();

    public void Set(string requested, string fallback, string @default)
    {
        RequestedLanguage = Normalize(requested);
        FallbackLanguage = Normalize(fallback);
        DefaultLanguage = Normalize(@default);
    }

    private static string Normalize(string lang) =>
        lang.Split('-', ';')[0].ToLowerInvariant().Trim();
}

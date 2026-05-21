namespace CMS.Application.Common.Abstractions;

public interface ILanguageContext
{
    string RequestedLanguage { get; }
    string FallbackLanguage { get; }
    string DefaultLanguage { get; }

    /// <summary>Öncelik sırasına göre dil adayları — Handler direkt kullanır.</summary>
    string[] LanguageCandidates { get; }

    void Set(string requested, string fallback, string @default);
}

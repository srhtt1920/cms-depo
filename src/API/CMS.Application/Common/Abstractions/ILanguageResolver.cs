namespace CMS.Application.Common.Abstractions;

/// <summary>
/// Tenant'ın fallback ve default dillerini çözer.
/// Implementasyon: Infrastructure/Caching/LanguageCacheService (cache destekli).
/// </summary>
public interface ILanguageResolver
{
    Task<(string Fallback, string Default)> ResolveAsync(Guid tenantId, CancellationToken ct = default);
}

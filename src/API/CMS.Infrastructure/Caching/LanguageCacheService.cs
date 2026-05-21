using CMS.Application.Common.Abstractions;
using CMS.Domain.Tenants;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace CMS.Infrastructure.Caching;

/// <summary>
/// Singleton servis — tenant dil listesini cache'ler.
/// ITenantRepository Scoped olduğu için IServiceScopeFactory üzerinden her çağrıda
/// kısa ömürlü bir scope açar. Cache hit olursa scope hiç açılmaz.
/// </summary>
public sealed class LanguageCacheService(
    ICacheService cache,
    IServiceScopeFactory scopeFactory)
    : ILanguageResolver
{
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(10);
    private static string Key(Guid tenantId) => $"lang:{tenantId}";

    // ── ILanguageResolver ────────────────────────────────────────

    public async Task<(string Fallback, string Default)> ResolveAsync(
        Guid tenantId, CancellationToken ct = default)
    {
        var langs = await GetSupportedLanguagesAsync(TenantId.From(tenantId), ct);

        var fallback = langs.FirstOrDefault(l => l.IsFallback)?.LanguageCode
                    ?? langs.FirstOrDefault(l => l.IsDefault)?.LanguageCode
                    ?? "en";

        var @default = langs.FirstOrDefault(l => l.IsDefault)?.LanguageCode ?? "en";

        return (fallback, @default);
    }

    // ── Public ──────────────────────────────────────────────────

    public async Task<IReadOnlyList<SupportedLanguage>> GetSupportedLanguagesAsync(
        TenantId tenantId, CancellationToken ct = default)
    {
        // Cache hit — scope açmaya gerek yok
        var cached = await cache.GetAsync<List<CachedLanguage>>(Key(tenantId.Value), ct);
        if (cached is not null)
            return cached
                .Select(l => SupportedLanguage.Create(tenantId, l.LanguageCode, l.IsDefault, l.IsFallback))
                .ToList();

        // Cache miss — Scoped repository için kısa scope aç
        using var scope = scopeFactory.CreateScope();
        var tenantRepo  = scope.ServiceProvider.GetRequiredService<ITenantRepository>();

        var languages = await tenantRepo.GetSupportedLanguagesAsync(tenantId, ct);

        await cache.SetAsync(
            Key(tenantId.Value),
            languages.Select(l => new CachedLanguage(l.LanguageCode, l.IsDefault, l.IsFallback)).ToList(),
            Ttl, ct);

        return languages;
    }

    public async Task InvalidateAsync(Guid tenantId, CancellationToken ct = default) =>
        await cache.RemoveAsync(Key(tenantId), ct);

    // ── Private ─────────────────────────────────────────────────

    private sealed record CachedLanguage(string LanguageCode, bool IsDefault, bool IsFallback);
}

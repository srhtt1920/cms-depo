using CMS.Application.Common.Abstractions;
using Microsoft.Extensions.Caching.Memory;

namespace CMS.Infrastructure.Caching;

/// <summary>
/// IMemoryCache implementasyonu.
/// Redis yokken veya tek instance ortamlarda kullanılır.
/// Set operasyonları HashSet ile simüle edilir.
/// </summary>
public sealed class MemoryCacheService(IMemoryCache cache) : ICacheService
{
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(5);

    public Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class
    {
        var val = cache.Get<T>(key);
        return Task.FromResult(val);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default) where T : class
    {
        cache.Set(key, value, expiry ?? DefaultTtl);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken ct = default)
    {
        cache.Remove(key);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key, CancellationToken ct = default) =>
        Task.FromResult(cache.TryGetValue(key, out _));

    public Task<bool> SetContainsAsync(string key, string member, CancellationToken ct = default)
    {
        var set = cache.Get<HashSet<string>>(key);
        return Task.FromResult(set?.Contains(member) ?? false);
    }

    public Task SetAddManyAsync(string key, IEnumerable<string> members, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        var set = new HashSet<string>(members);
        cache.Set(key, set, expiry ?? DefaultTtl);
        return Task.CompletedTask;
    }

    public Task SetIntAsync(string key, int value, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        cache.Set(key, value, expiry ?? DefaultTtl);
        return Task.CompletedTask;
    }

    public Task<int?> GetIntAsync(string key, CancellationToken ct = default)
    {
        if (cache.TryGetValue(key, out int v))
            return Task.FromResult<int?>(v);
        return Task.FromResult<int?>(null);
    }
}

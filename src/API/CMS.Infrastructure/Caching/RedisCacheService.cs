using CMS.Application.Common.Abstractions;
using StackExchange.Redis;
using System.Text.Json;

namespace CMS.Infrastructure.Caching;

public sealed class RedisCacheService(IConnectionMultiplexer redis) : ICacheService
{
    private readonly IDatabase _db = redis.GetDatabase();
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(5);

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class
    {
        var val = await _db.StringGetAsync(key);
        return val.HasValue ? JsonSerializer.Deserialize<T>(val!) : null;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default) where T : class
    {
        var json = JsonSerializer.Serialize(value);
        await _db.StringSetAsync(key, json, expiry ?? DefaultTtl);
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default) =>
        await _db.KeyDeleteAsync(key);

    public async Task<bool> ExistsAsync(string key, CancellationToken ct = default) =>
        await _db.KeyExistsAsync(key);

    public async Task<bool> SetContainsAsync(string key, string member, CancellationToken ct = default) =>
        await _db.SetContainsAsync(key, member);

    public async Task SetAddManyAsync(string key, IEnumerable<string> members, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        var tx = _db.CreateTransaction();
        _ = tx.KeyDeleteAsync(key);
        var arr = members.Select(m => (RedisValue)m).ToArray();
        if (arr.Length > 0) _ = tx.SetAddAsync(key, arr);
        _ = tx.KeyExpireAsync(key, expiry ?? DefaultTtl);
        await tx.ExecuteAsync();
    }

    public async Task SetIntAsync(string key, int value, TimeSpan? expiry = null, CancellationToken ct = default) =>
        await _db.StringSetAsync(key, value, expiry ?? DefaultTtl);

    public async Task<int?> GetIntAsync(string key, CancellationToken ct = default)
    {
        var val = await _db.StringGetAsync(key);
        return val.HasValue && int.TryParse(val, out var v) ? v : null;
    }
}

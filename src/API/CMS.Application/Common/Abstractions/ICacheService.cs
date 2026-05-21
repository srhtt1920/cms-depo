namespace CMS.Application.Common.Abstractions;

/// <summary>
/// Cache abstraction — Redis veya IMemoryCache implementasyonu seçilebilir.
/// Configuration'da "Cache:Provider" = "Redis" | "Memory" ile belirlenir.
/// </summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default) where T : class;
    Task RemoveAsync(string key, CancellationToken ct = default);
    Task<bool> ExistsAsync(string key, CancellationToken ct = default);

    /// <summary>Bir set içinde member var mı? (Permission key kontrolü için)</summary>
    Task<bool> SetContainsAsync(string key, string member, CancellationToken ct = default);

    /// <summary>Set'e birden fazla member ekle</summary>
    Task SetAddManyAsync(string key, IEnumerable<string> members, TimeSpan? expiry = null, CancellationToken ct = default);

    /// <summary>Integer değer set et</summary>
    Task SetIntAsync(string key, int value, TimeSpan? expiry = null, CancellationToken ct = default);

    /// <summary>Integer değer oku</summary>
    Task<int?> GetIntAsync(string key, CancellationToken ct = default);
}

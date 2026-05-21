using CMS.Application.Common.Abstractions;

namespace CMS.Infrastructure.Identity;

public sealed class TokenBlacklist(ICacheService cache) : ITokenBlacklist
{
    private static string Key(string jti) => $"blacklist:jti:{jti}";

    public async Task RevokeAsync(string jti, TimeSpan remaining, CancellationToken ct = default)
        => await cache.SetAsync(Key(jti), "1", remaining, ct);

    public async Task<bool> IsRevokedAsync(string jti, CancellationToken ct = default)
        => await cache.ExistsAsync(Key(jti), ct);
}

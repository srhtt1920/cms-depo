namespace CMS.Application.Common.Abstractions;

public interface ITokenBlacklist
{
    Task RevokeAsync(string jti, TimeSpan remaining, CancellationToken ct = default);
    Task<bool> IsRevokedAsync(string jti, CancellationToken ct = default);
}

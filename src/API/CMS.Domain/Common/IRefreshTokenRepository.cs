using CMS.Domain.Identity;

namespace CMS.Domain.Common;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default);
    Task AddAsync(RefreshToken token, CancellationToken ct = default);
    Task UpdateAsync(RefreshToken token, CancellationToken ct = default);
    Task RevokeAllForUserAsync(UserId userId, CancellationToken ct = default);
}

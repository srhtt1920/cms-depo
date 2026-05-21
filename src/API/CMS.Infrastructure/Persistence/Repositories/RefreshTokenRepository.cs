using CMS.Domain.Common;
using CMS.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace CMS.Infrastructure.Persistence.Repositories;

public sealed class RefreshTokenRepository(CmsDbContext context) : IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default) =>
        await context.Set<RefreshToken>()
            .FirstOrDefaultAsync(t => t.Token == token, ct);

    public async Task AddAsync(RefreshToken token, CancellationToken ct = default)
    {
        await context.Set<RefreshToken>().AddAsync(token, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(RefreshToken token, CancellationToken ct = default)
    {
        context.Set<RefreshToken>().Update(token);
        await context.SaveChangesAsync(ct);
    }

    public async Task RevokeAllForUserAsync(UserId userId, CancellationToken ct = default)
    {
        var tokens = await context.Set<RefreshToken>()
            .Where(t => t.UserId == userId && !t.IsRevoked)
            .ToListAsync(ct);
        foreach (var t in tokens) t.Revoke();
        await context.SaveChangesAsync(ct);
    }
}

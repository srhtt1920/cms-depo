using CMS.Domain.Common;

namespace CMS.Domain.Identity;

public sealed class RefreshToken : Entity<Guid>
{
    public UserId UserId { get; private set; } = default!;
    public string Token { get; private set; } = default!;
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? ReplacedBy { get; private set; }

    private RefreshToken() { }

    public static RefreshToken Create(UserId userId, int expiryDays = 30)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator
                            .GetBytes(64)),
            ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public bool IsActive => !IsRevoked && ExpiresAt > DateTime.UtcNow;

    public void Revoke(string? replacedByToken = null)
    {
        IsRevoked = true;
        ReplacedBy = replacedByToken;
    }
}

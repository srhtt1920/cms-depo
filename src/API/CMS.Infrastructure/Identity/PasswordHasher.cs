using CMS.Application.Common.Abstractions;

namespace CMS.Infrastructure.Identity;

public sealed class PasswordHasher : IPasswordHasher
{
    public bool Verify(string password, string hash) =>
        BCrypt.Net.BCrypt.Verify(password, hash);

    public string Hash(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
}

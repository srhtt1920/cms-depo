namespace CMS.Application.Common.Abstractions;

public interface IPasswordHasher
{
    bool Verify(string password, string hash);
    string Hash(string password);
}

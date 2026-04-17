using System.Security.Cryptography;
using System.Text;

namespace LifeAsAGame.Api.Infrastructure;

/// <summary>Lightweight salted hash — swap for ASP.NET Identity when you need production auth.</summary>
public static class PasswordHasher
{
    public static string Hash(string password)
    {
        var payload = Encoding.UTF8.GetBytes(password + "|life-as-a-game|v1");
        var hash = SHA256.HashData(payload);
        return Convert.ToHexString(hash);
    }

    public static bool Verify(string password, string storedHex) =>
        Hash(password).Equals(storedHex, StringComparison.Ordinal);
}

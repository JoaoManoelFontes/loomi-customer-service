using System.Security.Cryptography;
using CustomerService.Application.Abstractions;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace CustomerService.Infrastructure.Authentication;

public sealed class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100_000;

    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = DeriveKey(password, salt, Iterations);

        return $"PBKDF2.v1.{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }

        var parts = passwordHash.Split('.');

        if (parts.Length != 5 || parts[0] != "PBKDF2" || parts[1] != "v1")
        {
            return false;
        }

        if (!int.TryParse(parts[2], out var iterations))
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(parts[3]);
            var expectedHash = Convert.FromBase64String(parts[4]);
            var actualHash = DeriveKey(password, salt, iterations);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static byte[] DeriveKey(string password, byte[] salt, int iterations)
    {
        return KeyDerivation.Pbkdf2(
            password,
            salt,
            KeyDerivationPrf.HMACSHA256,
            iterations,
            HashSize);
    }
}

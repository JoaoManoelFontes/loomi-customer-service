using CustomerService.Application.Abstractions;

namespace CustomerService.UnitTests.Application;

internal sealed class FakePasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        return $"hashed:{password}";
    }

    public bool Verify(string password, string passwordHash)
    {
        return passwordHash == Hash(password);
    }
}

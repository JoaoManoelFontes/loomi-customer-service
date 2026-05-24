using CustomerService.Domain.Enums;
using CustomerService.Domain.Exceptions;

namespace CustomerService.Domain.Entities;

public sealed class User
{
    private User()
    {
        Cpf = string.Empty;
        PasswordHash = string.Empty;
    }

    public User(string cpf, string passwordHash, UserRole role, Guid? customerId = null)
        : this(Guid.NewGuid(), cpf, passwordHash, role, customerId, DateTime.UtcNow, null)
    {
    }

    internal User(
        Guid id,
        string cpf,
        string passwordHash,
        UserRole role,
        Guid? customerId,
        DateTime createdAt,
        DateTime? updatedAt)
    {
        Id = id == Guid.Empty ? throw new DomainValidationException("User id is required.") : id;
        Cpf = RequireValue(cpf, nameof(Cpf));
        PasswordHash = RequireValue(passwordHash, nameof(PasswordHash));
        Role = role;
        CustomerId = customerId;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public Guid Id { get; private set; }

    public string Cpf { get; private set; } = null!;

    public string PasswordHash { get; private set; } = null!;

    public UserRole Role { get; private set; }

    public Guid? CustomerId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    public void ChangePasswordHash(string passwordHash)
    {
        PasswordHash = RequireValue(passwordHash, nameof(PasswordHash));
        UpdatedAt = DateTime.UtcNow;
    }

    private static string RequireValue(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainValidationException($"{fieldName} is required.");
        }

        return value.Trim();
    }
}

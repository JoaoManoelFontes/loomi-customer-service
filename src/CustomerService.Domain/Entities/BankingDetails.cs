using CustomerService.Domain.Exceptions;

namespace CustomerService.Domain.Entities;

public sealed class BankingDetails
{
    private BankingDetails()
    {
        Agency = string.Empty;
        CheckingAccountNumber = string.Empty;
    }

    public BankingDetails(string agency, string checkingAccountNumber, decimal balance)
        : this(Guid.NewGuid(), agency, checkingAccountNumber, balance)
    {
    }

    internal BankingDetails(Guid id, string agency, string checkingAccountNumber, decimal balance)
    {
        Id = id == Guid.Empty ? throw new DomainValidationException("Banking details id is required.") : id;
        Agency = RequireValue(agency, nameof(Agency));
        CheckingAccountNumber = RequireValue(checkingAccountNumber, nameof(CheckingAccountNumber));
        Balance = balance < 0 ? throw new DomainValidationException("Balance cannot be negative.") : balance;
    }

    public Guid Id { get; private set; }

    public Guid CustomerId { get; private set; }

    public string Agency { get; private set; } = null!;

    public string CheckingAccountNumber { get; private set; } = null!;

    public decimal Balance { get; private set; }

    public void Update(string? agency, string? checkingAccountNumber)
    {
        if (agency is not null)
        {
            Agency = RequireValue(agency, nameof(Agency));
        }

        if (checkingAccountNumber is not null)
        {
            CheckingAccountNumber = RequireValue(checkingAccountNumber, nameof(CheckingAccountNumber));
        }
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

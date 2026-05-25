using CustomerService.Domain.Exceptions;

namespace CustomerService.Domain.Entities;

public sealed class Customer
{
    private Customer()
    {
        Name = string.Empty;
        Email = string.Empty;
        Address = string.Empty;
        BankingDetails = null!;
    }

    public Customer(
        string name,
        string email,
        string address,
        BankingDetails bankingDetails,
        string? profilePictureUrl = null)
        : this(Guid.NewGuid(), name, email, address, bankingDetails, profilePictureUrl)
    {
    }

    internal Customer(
        Guid id,
        string name,
        string email,
        string address,
        BankingDetails bankingDetails,
        string? profilePictureUrl = null)
    {
        Id = id == Guid.Empty ? throw new DomainValidationException("Customer id is required.") : id;
        SetProfileData(name, email, address);
        BankingDetails = bankingDetails ?? throw new DomainValidationException("Banking details are required.");
        ProfilePictureUrl = NormalizeOptional(profilePictureUrl);
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; }  = null!;

    public string Email { get; private set; }  = null!;

    public string Address { get; private set; }  = null!;

    public string? ProfilePictureUrl { get; private set; }

    public BankingDetails BankingDetails { get; private set; }

    public void UpdateProfile(string name, string email, string address)
    {
        SetProfileData(name, email, address);
    }

    public void UpdateProfileFields(string? name, string? email, string? address)
    {
        if (name is not null)
        {
            Name = RequireValue(name, nameof(Name));
        }

        if (email is not null)
        {
            Email = RequireValue(email, nameof(Email));
        }

        if (address is not null)
        {
            Address = RequireValue(address, nameof(Address));
        }
    }

    public void UpdateBankingDetails(string? agency, string? checkingAccountNumber)
    {
        BankingDetails.Update(agency, checkingAccountNumber);
    }

    public void UpdateProfilePicture(string? profilePictureUrl)
    {
        ProfilePictureUrl = NormalizeOptional(profilePictureUrl);
    }

    private void SetProfileData(string name, string email, string address)
    {
        Name = RequireValue(name, nameof(Name));
        Email = RequireValue(email, nameof(Email));
        Address = RequireValue(address, nameof(Address));
    }

    private static string RequireValue(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainValidationException($"{fieldName} is required.");
        }

        return value.Trim();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}

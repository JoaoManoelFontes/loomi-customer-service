using CustomerService.Domain.Entities;
using CustomerService.Domain.Exceptions;
using FluentAssertions;

namespace CustomerService.UnitTests.Domain.Entities;

public sealed class BankingDetailsTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateBankingDetails()
    {
        var bankingDetails = new BankingDetails(" 0001 ", " 123456-7 ", 250.75m);

        bankingDetails.Id.Should().NotBeEmpty();
        bankingDetails.Agency.Should().Be("0001");
        bankingDetails.CheckingAccountNumber.Should().Be("123456-7");
        bankingDetails.Balance.Should().Be(250.75m);
    }

    [Theory]
    [InlineData("", "123456-7", 0, "Agency is required.")]
    [InlineData(" ", "123456-7", 0, "Agency is required.")]
    [InlineData("0001", "", 0, "CheckingAccountNumber is required.")]
    [InlineData("0001", " ", 0, "CheckingAccountNumber is required.")]
    [InlineData("0001", "123456-7", -1, "Balance cannot be negative.")]
    public void Constructor_WithInvalidData_ShouldThrowDomainValidationException(
        string agency,
        string checkingAccountNumber,
        decimal balance,
        string expectedMessage)
    {
        var act = () => new BankingDetails(agency, checkingAccountNumber, balance);

        act.Should().Throw<DomainValidationException>()
            .WithMessage(expectedMessage);
    }

    [Fact]
    public void Debit_WithAvailableBalance_ShouldDecreaseBalance()
    {
        var bankingDetails = new BankingDetails("0001", "123456-7", 250.75m);

        bankingDetails.Debit(50.25m);

        bankingDetails.Balance.Should().Be(200.50m);
    }

    [Fact]
    public void Credit_WithPositiveAmount_ShouldIncreaseBalance()
    {
        var bankingDetails = new BankingDetails("0001", "123456-7", 250.75m);

        bankingDetails.Credit(49.25m);

        bankingDetails.Balance.Should().Be(300m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Debit_WithInvalidAmount_ShouldThrowDomainValidationException(decimal amount)
    {
        var bankingDetails = new BankingDetails("0001", "123456-7", 250.75m);

        var act = () => bankingDetails.Debit(amount);

        act.Should().Throw<DomainValidationException>()
            .WithMessage("Amount must be greater than zero.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Credit_WithInvalidAmount_ShouldThrowDomainValidationException(decimal amount)
    {
        var bankingDetails = new BankingDetails("0001", "123456-7", 250.75m);

        var act = () => bankingDetails.Credit(amount);

        act.Should().Throw<DomainValidationException>()
            .WithMessage("Amount must be greater than zero.");
    }

    [Fact]
    public void Debit_WithInsufficientBalance_ShouldThrowDomainValidationException()
    {
        var bankingDetails = new BankingDetails("0001", "123456-7", 100m);

        var act = () => bankingDetails.Debit(100.01m);

        act.Should().Throw<DomainValidationException>()
            .WithMessage("Insufficient balance.");
    }
}

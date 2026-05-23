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
}

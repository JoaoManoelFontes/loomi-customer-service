using CustomerService.Application.Customers.UpdateCustomer;
using FluentAssertions;

namespace CustomerService.UnitTests.Application.Customers;

public sealed class UpdateCustomerRequestValidatorTests
{
    private readonly UpdateCustomerRequestValidator _validator = new();

    [Fact]
    public void Validate_WithOneProfileField_ShouldBeValid()
    {
        var result = _validator.Validate(new UpdateCustomerRequest(
            Name: "Maria Silva",
            Email: null,
            Address: null,
            BankingDetails: null));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithOneBankingField_ShouldBeValid()
    {
        var result = _validator.Validate(new UpdateCustomerRequest(
            Name: null,
            Email: null,
            Address: null,
            BankingDetails: new UpdateBankingDetailsRequest("0002", null)));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithoutSupportedFields_ShouldBeInvalid()
    {
        var result = _validator.Validate(new UpdateCustomerRequest(
            Name: null,
            Email: null,
            Address: null,
            BankingDetails: null));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.ErrorMessage == "At least one supported field must be provided.");
    }

    [Theory]
    [InlineData("", "maria.silva@example.com", "Rua A, 123", "0001", "123456-7")]
    [InlineData("Maria Silva", "invalid-email", "Rua A, 123", "0001", "123456-7")]
    [InlineData("Maria Silva", "maria.silva@example.com", "", "0001", "123456-7")]
    [InlineData("Maria Silva", "maria.silva@example.com", "Rua A, 123", "", "123456-7")]
    [InlineData("Maria Silva", "maria.silva@example.com", "Rua A, 123", "0001", "")]
    public void Validate_WithInvalidSuppliedFields_ShouldBeInvalid(
        string name,
        string email,
        string address,
        string agency,
        string checkingAccountNumber)
    {
        var result = _validator.Validate(new UpdateCustomerRequest(
            name,
            email,
            address,
            new UpdateBankingDetailsRequest(agency, checkingAccountNumber)));

        result.IsValid.Should().BeFalse();
    }
}

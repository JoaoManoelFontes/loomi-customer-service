using CustomerService.Domain.Entities;
using CustomerService.Domain.Enums;
using CustomerService.Domain.Exceptions;
using FluentAssertions;

namespace CustomerService.UnitTests.Domain.Entities;

public sealed class UserTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateUser()
    {
        var customerId = Guid.NewGuid();

        var user = new User(" 12345678900 ", " hashed-password ", UserRole.Customer, customerId);

        user.Id.Should().NotBeEmpty();
        user.Cpf.Should().Be("12345678900");
        user.PasswordHash.Should().Be("hashed-password");
        user.Role.Should().Be(UserRole.Customer);
        user.CustomerId.Should().Be(customerId);
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        user.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithoutCustomerId_ShouldCreateUser()
    {
        var user = new User("00000000000", "hashed-password", UserRole.Admin);

        user.CustomerId.Should().BeNull();
    }

    [Theory]
    [InlineData("", "hashed-password", "Cpf is required.")]
    [InlineData(" ", "hashed-password", "Cpf is required.")]
    [InlineData("12345678900", "", "PasswordHash is required.")]
    [InlineData("12345678900", " ", "PasswordHash is required.")]
    public void Constructor_WithInvalidRequiredData_ShouldThrowDomainValidationException(
        string cpf,
        string passwordHash,
        string expectedMessage)
    {
        var act = () => new User(cpf, passwordHash, UserRole.Customer);

        act.Should().Throw<DomainValidationException>()
            .WithMessage(expectedMessage);
    }
}

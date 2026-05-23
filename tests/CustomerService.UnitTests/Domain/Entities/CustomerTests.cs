using CustomerService.Domain.Entities;
using CustomerService.Domain.Exceptions;
using FluentAssertions;

namespace CustomerService.UnitTests.Domain.Entities;

public sealed class CustomerTests
{
    [Fact]
    public void Constructor_WithValidRequiredProfileData_ShouldCreateCustomer()
    {
        var customer = new Customer(
            "Ada Lovelace",
            "ada@example.com",
            "123 Main Street");

        customer.Id.Should().NotBeEmpty();
        customer.Name.Should().Be("Ada Lovelace");
        customer.Email.Should().Be("ada@example.com");
        customer.Address.Should().Be("123 Main Street");
    }

    [Fact]
    public void Constructor_WithoutProfilePictureUrl_ShouldCreateCustomerWithNullProfilePictureUrl()
    {
        var customer = new Customer(
            "Ada Lovelace",
            "ada@example.com",
            "123 Main Street");

        customer.ProfilePictureUrl.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WithMissingName_ShouldThrowDomainValidationException(string name)
    {
        var act = () => new Customer(name, "ada@example.com", "123 Main Street");

        act.Should().Throw<DomainValidationException>()
            .WithMessage("Name is required.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WithMissingEmail_ShouldThrowDomainValidationException(string email)
    {
        var act = () => new Customer("Ada Lovelace", email, "123 Main Street");

        act.Should().Throw<DomainValidationException>()
            .WithMessage("Email is required.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WithMissingAddress_ShouldThrowDomainValidationException(string address)
    {
        var act = () => new Customer("Ada Lovelace", "ada@example.com", address);

        act.Should().Throw<DomainValidationException>()
            .WithMessage("Address is required.");
    }

    [Fact]
    public void UpdateProfile_WithValidProfileData_ShouldUpdateCustomer()
    {
        var customer = CreateCustomer();

        customer.UpdateProfile(
            "Grace Hopper",
            "grace@example.com",
            "456 Ocean Avenue");

        customer.Name.Should().Be("Grace Hopper");
        customer.Email.Should().Be("grace@example.com");
        customer.Address.Should().Be("456 Ocean Avenue");
    }

    [Fact]
    public void UpdateProfilePicture_WithValidUrl_ShouldUpdateProfilePictureUrl()
    {
        var customer = CreateCustomer();

        customer.UpdateProfilePicture("https://storage.example.com/customers/profile.png");

        customer.ProfilePictureUrl.Should().Be("https://storage.example.com/customers/profile.png");
    }

    [Fact]
    public void UpdateProfilePicture_WithBlankUrl_ShouldClearProfilePictureUrl()
    {
        var customer = new Customer(
            "Ada Lovelace",
            "ada@example.com",
            "123 Main Street",
            "https://storage.example.com/customers/profile.png");

        customer.UpdateProfilePicture(" ");

        customer.ProfilePictureUrl.Should().BeNull();
    }

    [Theory]
    [InlineData("", "ada@example.com", "123 Main Street", "Name is required.")]
    [InlineData("Ada Lovelace", "", "123 Main Street", "Email is required.")]
    [InlineData("Ada Lovelace", "ada@example.com", "", "Address is required.")]
    public void UpdateProfile_WithInvalidRequiredProfileData_ShouldThrowDomainValidationException(
        string name,
        string email,
        string address,
        string expectedMessage)
    {
        var customer = CreateCustomer();

        var act = () => customer.UpdateProfile(name, email, address);

        act.Should().Throw<DomainValidationException>()
            .WithMessage(expectedMessage);
    }

    private static Customer CreateCustomer()
    {
        return new Customer(
            "Ada Lovelace",
            "ada@example.com",
            "123 Main Street");
    }
}

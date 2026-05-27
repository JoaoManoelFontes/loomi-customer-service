using CustomerService.Application.Users.CreateUser;
using CustomerService.Application.Common.Exceptions;
using CustomerService.Domain.Entities;
using CustomerService.Domain.Enums;
using CustomerService.UnitTests.Application;
using FluentAssertions;

namespace CustomerService.UnitTests.Application.Users;

public sealed class CreateUserHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidData_ShouldCreateUserAndReturnResponseWithoutPasswordHash()
    {
        var repository = new FakeUserRepository();
        var handler = new CreateUserHandler(repository, new FakePasswordHasher(), new CreateUserRequestValidator());

        var response = await handler.HandleAsync(new CreateCustomerRequest(
            "123.456.789-00",
            "Customer@123",
            "Customer",
            "Maria Silva",
            "maria.silva@example.com",
            "Rua A, 123",
            "0001",
            "123456-7",
            250.75m));

        response.Id.Should().NotBeEmpty();
        response.Cpf.Should().Be("12345678900");
        response.Role.Should().Be(UserRole.Customer);
        response.Customer.Should().NotBeNull();
        response.Customer.Id.Should().NotBeNull();
        response.Customer.Name.Should().Be("Maria Silva");
        response.Customer.Email.Should().Be("maria.silva@example.com");
        response.Customer.Address.Should().Be("Rua A, 123");
        response.Customer.Agency.Should().Be("0001");
        response.Customer.CheckingAccountNumber.Should().Be("123456-7");
        response.Customer.Balance.Should().Be(250.75m);
        repository.AddedUser.Should().NotBeNull();
        repository.AddedUser!.PasswordHash.Should().Be("hashed:Customer@123");
        repository.AddedUser.CustomerId.Should().Be(response.Customer.Id);
        repository.AddedCustomer.Should().NotBeNull();
        repository.AddedCustomer!.Id.Should().Be(response.Customer.Id!.Value);
        repository.AddedCustomer.Name.Should().Be("Maria Silva");
        repository.AddedCustomer.Email.Should().Be("maria.silva@example.com");
        repository.AddedCustomer.Address.Should().Be("Rua A, 123");
        repository.AddedCustomer.BankingDetails.Agency.Should().Be("0001");
        repository.AddedCustomer.BankingDetails.CheckingAccountNumber.Should().Be("123456-7");
        repository.AddedCustomer.BankingDetails.Balance.Should().Be(250.75m);
    }

    [Fact]
    public async Task HandleAsync_WithDuplicateCpf_ShouldThrowDuplicateCpfException()
    {
        var existingUser = new User("12345678900", "hashed:Customer@123", UserRole.Customer);
        var handler = new CreateUserHandler(
            new FakeUserRepository(existingUser),
            new FakePasswordHasher(),
            new CreateUserRequestValidator());

        var act = () => handler.HandleAsync(new CreateCustomerRequest(
            "12345678900",
            "Customer@123",
            "Customer",
            "Maria Silva",
            "maria.silva@example.com",
            "Rua A, 123",
            "0001",
            "123456-7",
            0));

        await act.Should().ThrowAsync<DuplicateCpfException>();
    }

    [Theory]
    [InlineData("", "Customer@123", "Customer", "Maria Silva", "maria.silva@example.com", "Rua A, 123", "0001", "123456-7", 0)]
    [InlineData("123", "Customer@123", "Customer", "Maria Silva", "maria.silva@example.com", "Rua A, 123", "0001", "123456-7", 0)]
    [InlineData("12345678900", "short", "Customer", "Maria Silva", "maria.silva@example.com", "Rua A, 123", "0001", "123456-7", 0)]
    [InlineData("12345678900", "Customer@123", "Unknown", "Maria Silva", "maria.silva@example.com", "Rua A, 123", "0001", "123456-7", 0)]
    [InlineData("12345678900", "Customer@123", "Customer", "", "maria.silva@example.com", "Rua A, 123", "0001", "123456-7", 0)]
    [InlineData("12345678900", "Customer@123", "Customer", "Maria Silva", "invalid-email", "Rua A, 123", "0001", "123456-7", 0)]
    [InlineData("12345678900", "Customer@123", "Customer", "Maria Silva", "maria.silva@example.com", "", "0001", "123456-7", 0)]
    [InlineData("12345678900", "Customer@123", "Customer", "Maria Silva", "maria.silva@example.com", "Rua A, 123", "", "123456-7", 0)]
    [InlineData("12345678900", "Customer@123", "Customer", "Maria Silva", "maria.silva@example.com", "Rua A, 123", "0001", "", 0)]
    [InlineData("12345678900", "Customer@123", "Customer", "Maria Silva", "maria.silva@example.com", "Rua A, 123", "0001", "123456-7", -1)]
    public async Task HandleAsync_WithInvalidData_ShouldThrowValidationException(
        string cpf,
        string password,
        string role,
        string name,
        string email,
        string address,
        string agency,
        string checkingAccountNumber,
        decimal balance)
    {
        var repository = new FakeUserRepository();
        var handler = new CreateUserHandler(repository, new FakePasswordHasher(), new CreateUserRequestValidator());

        var act = () => handler.HandleAsync(new CreateCustomerRequest(
            cpf,
            password,
            role,
            name,
            email,
            address,
            agency,
            checkingAccountNumber,
            balance));

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
        repository.AddedUser.Should().BeNull();
        repository.AddedCustomer.Should().BeNull();
    }
}

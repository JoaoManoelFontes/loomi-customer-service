using CustomerService.Application.Common.Exceptions;
using CustomerService.Application.Customers.UpdateCustomer;
using CustomerService.Domain.Entities;
using CustomerService.UnitTests.Application;
using FluentAssertions;

namespace CustomerService.UnitTests.Application.Customers;

public sealed class UpdateCustomerHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithProfileSubset_ShouldUpdateOnlySuppliedFields()
    {
        var customer = CreateCustomer();
        var repository = new FakeCustomerRepository(customer);
        var handler = new UpdateCustomerHandler(repository, new UpdateCustomerRequestValidator());

        var response = await handler.HandleAsync(
            customer.Id,
            new UpdateCustomerRequest("Ana Souza", null, null, null));

        response.CustomerId.Should().Be(customer.Id);
        response.Status.Should().Be("updated");
        repository.UpdatedCustomer.Should().Be(customer);
        customer.Name.Should().Be("Ana Souza");
        customer.Email.Should().Be("maria.silva@example.com");
        customer.Address.Should().Be("Rua A, 123");
        customer.BankingDetails.Agency.Should().Be("0001");
        customer.BankingDetails.CheckingAccountNumber.Should().Be("123456-7");
    }

    [Fact]
    public async Task HandleAsync_WithBankingDetailsSubset_ShouldUpdateOnlySuppliedBankingFields()
    {
        var customer = CreateCustomer();
        var repository = new FakeCustomerRepository(customer);
        var handler = new UpdateCustomerHandler(repository, new UpdateCustomerRequestValidator());

        await handler.HandleAsync(
            customer.Id,
            new UpdateCustomerRequest(
                null,
                null,
                null,
                new UpdateBankingDetailsRequest("0002", null)));

        customer.Name.Should().Be("Maria Silva");
        customer.BankingDetails.Agency.Should().Be("0002");
        customer.BankingDetails.CheckingAccountNumber.Should().Be("123456-7");
        repository.UpdatedCustomer.Should().Be(customer);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidRequest_ShouldThrowValidationException()
    {
        var customer = CreateCustomer();
        var repository = new FakeCustomerRepository(customer);
        var handler = new UpdateCustomerHandler(repository, new UpdateCustomerRequestValidator());

        var act = () => handler.HandleAsync(
            customer.Id,
            new UpdateCustomerRequest(null, "invalid-email", null, null));

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
        repository.UpdatedCustomer.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WithMissingCustomer_ShouldThrowCustomerNotFoundException()
    {
        var customerId = Guid.NewGuid();
        var repository = new FakeCustomerRepository();
        var handler = new UpdateCustomerHandler(repository, new UpdateCustomerRequestValidator());

        var act = () => handler.HandleAsync(
            customerId,
            new UpdateCustomerRequest("Ana Souza", null, null, null));

        var exception = await act.Should().ThrowAsync<CustomerNotFoundException>();
        exception.Which.CustomerId.Should().Be(customerId);
        repository.UpdatedCustomer.Should().BeNull();
    }

    private static Customer CreateCustomer()
    {
        return new Customer(
            "Maria Silva",
            "maria.silva@example.com",
            "Rua A, 123",
            new BankingDetails("0001", "123456-7", 150.25m));
    }
}

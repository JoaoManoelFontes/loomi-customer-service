using CustomerService.Application.Customers.Exists;
using CustomerService.Domain.Entities;
using CustomerService.UnitTests.Application;
using FluentAssertions;

namespace CustomerService.UnitTests.Application.Customers;

public sealed class CustomerExistsHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithExistingCustomer_ShouldReturnTrue()
    {
        var customer = new Customer(
            "Maria Silva",
            "maria.silva@example.com",
            "Rua A, 123",
            new BankingDetails("0001", "123456-7", 150.25m));
        var handler = new CustomerExistsHandler(new FakeCustomerRepository(customer));

        var exists = await handler.HandleAsync(customer.Id);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithMissingCustomer_ShouldReturnFalse()
    {
        var handler = new CustomerExistsHandler(new FakeCustomerRepository());

        var exists = await handler.HandleAsync(Guid.NewGuid());

        exists.Should().BeFalse();
    }
}

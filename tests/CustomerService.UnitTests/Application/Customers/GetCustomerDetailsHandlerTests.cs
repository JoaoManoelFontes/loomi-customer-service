using CustomerService.Application.Common.Exceptions;
using CustomerService.Application.Customers.GetCustomerDetails;
using CustomerService.Domain.Entities;
using CustomerService.UnitTests.Application;
using FluentAssertions;

namespace CustomerService.UnitTests.Application.Customers;

public sealed class GetCustomerDetailsHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithExistingCustomer_ShouldReturnCustomerDetails()
    {
        var customer = new Customer(
            "Maria Silva",
            "maria.silva@example.com",
            "Rua A, 123",
            new BankingDetails("0001", "123456-7", 150.25m),
            "https://example.com/profile.jpg");
        var handler = new GetCustomerDetailsHandler(new FakeCustomerRepository(customer));

        var response = await handler.HandleAsync(customer.Id);

        response.Id.Should().Be(customer.Id);
        response.Name.Should().Be("Maria Silva");
        response.Email.Should().Be("maria.silva@example.com");
        response.Address.Should().Be("Rua A, 123");
        response.ProfilePictureUrl.Should().Be("https://example.com/profile.jpg");
        response.BankingDetails.Agency.Should().Be("0001");
        response.BankingDetails.CheckingAccountNumber.Should().Be("123456-7");
        response.BankingDetails.Balance.Should().Be(150.25m);
    }

    [Fact]
    public async Task HandleAsync_WithMissingCustomer_ShouldThrowCustomerNotFoundException()
    {
        var customerId = Guid.NewGuid();
        var handler = new GetCustomerDetailsHandler(new FakeCustomerRepository());

        var act = () => handler.HandleAsync(customerId);

        var exception = await act.Should().ThrowAsync<CustomerNotFoundException>();
        exception.Which.CustomerId.Should().Be(customerId);
    }
}

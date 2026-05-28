using CustomerService.Application.Customers.Exists;
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
        var handler = CreateHandler(repository);

        var response = await handler.HandleAsync(
            customer.Id,
            new UpdateCustomerRequest("Ana Souza", null, null, null));

        response.CustomerId.Should().Be(customer.Id);
        response.Status.Should().Be("updated");
        repository.UpdatedCustomer.Should().Be(customer);
        customer.Name.Should().Be("Ana Souza");
        customer.Email.Should().Be("maria.silva@example.com");
        customer.Address.Should().Be("Rua A, 123");
        customer.ProfilePictureUrl.Should().BeNull();
        customer.BankingDetails.Agency.Should().Be("0001");
        customer.BankingDetails.CheckingAccountNumber.Should().Be("123456-7");
    }

    [Fact]
    public async Task HandleAsync_WithProfileImageUrl_ShouldUpdateProfilePictureUrl()
    {
        var customer = CreateCustomer();
        var repository = new FakeCustomerRepository(customer);
        var handler = CreateHandler(repository);

        await handler.HandleAsync(
            customer.Id,
            new UpdateCustomerRequest(
                null,
                null,
                null,
                null,
                "https://storage.example.com/customers/profile.png"));

        customer.ProfilePictureUrl.Should().Be("https://storage.example.com/customers/profile.png");
        repository.UpdatedCustomer.Should().Be(customer);
    }

    [Fact]
    public async Task HandleAsync_WithBankingDetailsSubset_ShouldUpdateOnlySuppliedBankingFields()
    {
        var customer = CreateCustomer();
        var repository = new FakeCustomerRepository(customer);
        var handler = CreateHandler(repository);

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
        var handler = CreateHandler(repository);

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
        var handler = CreateHandler(repository);

        var act = () => handler.HandleAsync(
            customerId,
            new UpdateCustomerRequest("Ana Souza", null, null, null));

        var exception = await act.Should().ThrowAsync<CustomerNotFoundException>();
        exception.Which.CustomerId.Should().Be(customerId);
        repository.UpdatedCustomer.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WhenUpdateSucceeds_ShouldRefreshCustomerDetailsCache()
    {
        var customer = CreateCustomer();
        var repository = new FakeCustomerRepository(customer);
        var cache = new FakeCustomerDetailsCache();
        var handler = CreateHandler(repository, cache);

        await handler.HandleAsync(
            customer.Id,
            new UpdateCustomerRequest("Ana Souza", null, null, null));

        cache.WrittenCustomerId.Should().Be(customer.Id);
        cache.WrittenValue.Should().NotBeNull();
        cache.WrittenValue!.Name.Should().Be("Ana Souza");
        cache.WrittenValue.BankingDetails.Balance.Should().Be(150.25m);
        cache.WrittenExpiration.Should().Be(TimeSpan.FromSeconds(123));
    }

    [Fact]
    public async Task HandleAsync_WhenCacheRefreshFails_ShouldReturnSuccessfulResponse()
    {
        var customer = CreateCustomer();
        var repository = new FakeCustomerRepository(customer);
        var cache = new FakeCustomerDetailsCache { ThrowOnSet = true };
        var handler = CreateHandler(repository, cache);

        var response = await handler.HandleAsync(
            customer.Id,
            new UpdateCustomerRequest("Ana Souza", null, null, null));

        response.CustomerId.Should().Be(customer.Id);
        response.Status.Should().Be("updated");
        repository.UpdatedCustomer.Should().Be(customer);
    }

    private static Customer CreateCustomer()
    {
        return new Customer(
            "Maria Silva",
            "maria.silva@example.com",
            "Rua A, 123",
            new BankingDetails("0001", "123456-7", 150.25m));
    }

    private static UpdateCustomerHandler CreateHandler(
        FakeCustomerRepository repository,
        FakeCustomerDetailsCache? cache = null)
    {
        return new UpdateCustomerHandler(
            repository,
            cache ?? new FakeCustomerDetailsCache(),
            new CustomerExistenceCacheOptions { DetailsTtlSeconds = 123 },
            new UpdateCustomerRequestValidator());
    }
}

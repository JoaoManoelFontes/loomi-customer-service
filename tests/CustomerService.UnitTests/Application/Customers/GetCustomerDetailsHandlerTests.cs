using CustomerService.Application.Customers.Exists;
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
        var handler = CreateHandler(new FakeCustomerRepository(customer));

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
        var handler = CreateHandler(new FakeCustomerRepository());

        var act = () => handler.HandleAsync(customerId);

        var exception = await act.Should().ThrowAsync<CustomerNotFoundException>();
        exception.Which.CustomerId.Should().Be(customerId);
    }

    [Fact]
    public async Task HandleAsync_WithCachedDetails_ShouldReturnCachedValueWithoutRepositoryLookup()
    {
        var customerId = Guid.NewGuid();
        var repository = new FakeCustomerRepository();
        var cached = new CustomerDetailsResponse(
            customerId,
            "Cached Customer",
            "cached@example.com",
            "Cached Address",
            null,
            new CustomerBankingDetailsResponse("0001", "123456-7", 150.25m));
        var cache = new FakeCustomerDetailsCache { CachedValue = cached };
        var handler = CreateHandler(repository, cache);

        var response = await handler.HandleAsync(customerId);

        response.Should().Be(cached);
        repository.GetByIdCallCount.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_WithCacheMiss_ShouldQueryRepositoryAndWriteCacheWithTtl()
    {
        var customer = new Customer(
            "Maria Silva",
            "maria.silva@example.com",
            "Rua A, 123",
            new BankingDetails("0001", "123456-7", 150.25m));
        var repository = new FakeCustomerRepository(customer);
        var cache = new FakeCustomerDetailsCache();
        var handler = CreateHandler(repository, cache);

        var response = await handler.HandleAsync(customer.Id);

        repository.GetByIdCallCount.Should().Be(1);
        cache.WrittenCustomerId.Should().Be(customer.Id);
        cache.WrittenValue.Should().Be(response);
        cache.WrittenExpiration.Should().Be(TimeSpan.FromSeconds(123));
    }

    [Fact]
    public async Task HandleAsync_WhenCacheReadFails_ShouldReturnRepositoryResult()
    {
        var customer = new Customer(
            "Maria Silva",
            "maria.silva@example.com",
            "Rua A, 123",
            new BankingDetails("0001", "123456-7", 150.25m));
        var repository = new FakeCustomerRepository(customer);
        var cache = new FakeCustomerDetailsCache { ThrowOnGet = true };
        var handler = CreateHandler(repository, cache);

        var response = await handler.HandleAsync(customer.Id);

        response.Id.Should().Be(customer.Id);
        repository.GetByIdCallCount.Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_WhenCacheWriteFails_ShouldReturnRepositoryResult()
    {
        var customer = new Customer(
            "Maria Silva",
            "maria.silva@example.com",
            "Rua A, 123",
            new BankingDetails("0001", "123456-7", 150.25m));
        var repository = new FakeCustomerRepository(customer);
        var cache = new FakeCustomerDetailsCache { ThrowOnSet = true };
        var handler = CreateHandler(repository, cache);

        var response = await handler.HandleAsync(customer.Id);

        response.Id.Should().Be(customer.Id);
        repository.GetByIdCallCount.Should().Be(1);
    }

    private static GetCustomerDetailsHandler CreateHandler(
        FakeCustomerRepository repository,
        FakeCustomerDetailsCache? cache = null)
    {
        return new GetCustomerDetailsHandler(
            repository,
            cache ?? new FakeCustomerDetailsCache(),
            new CustomerExistenceCacheOptions { DetailsTtlSeconds = 123 });
    }
}

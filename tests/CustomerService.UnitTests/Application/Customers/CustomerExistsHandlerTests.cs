using CustomerService.Application.Abstractions;
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
        var handler = CreateHandler(new FakeCustomerRepository(customer));

        var exists = await handler.HandleAsync(customer.Id);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithMissingCustomer_ShouldReturnFalse()
    {
        var handler = CreateHandler(new FakeCustomerRepository());

        var exists = await handler.HandleAsync(Guid.NewGuid());

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WithCachedResult_ShouldReturnCachedValueWithoutRepositoryLookup()
    {
        var repository = new FakeCustomerRepository();
        var cache = new FakeCustomerExistenceCache { CachedValue = true };
        var handler = CreateHandler(repository, cache);

        var exists = await handler.HandleAsync(Guid.NewGuid());

        exists.Should().BeTrue();
        repository.ExistsCallCount.Should().Be(0);
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
        var cache = new FakeCustomerExistenceCache();
        var handler = CreateHandler(repository, cache);

        var exists = await handler.HandleAsync(customer.Id);

        exists.Should().BeTrue();
        repository.ExistsCallCount.Should().Be(1);
        cache.WrittenCustomerId.Should().Be(customer.Id);
        cache.WrittenValue.Should().BeTrue();
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
        var cache = new FakeCustomerExistenceCache { ThrowOnGet = true };
        var handler = CreateHandler(repository, cache);

        var exists = await handler.HandleAsync(customer.Id);

        exists.Should().BeTrue();
        repository.ExistsCallCount.Should().Be(1);
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
        var cache = new FakeCustomerExistenceCache { ThrowOnSet = true };
        var handler = CreateHandler(repository, cache);

        var exists = await handler.HandleAsync(customer.Id);

        exists.Should().BeTrue();
        repository.ExistsCallCount.Should().Be(1);
    }

    private static CustomerExistsHandler CreateHandler(
        FakeCustomerRepository repository,
        ICustomerExistenceCache? cache = null)
    {
        return new CustomerExistsHandler(
            repository,
            cache ?? new FakeCustomerExistenceCache(),
            new CustomerExistenceCacheOptions { ExistsTtlSeconds = 123 });
    }

    private sealed class FakeCustomerExistenceCache : ICustomerExistenceCache
    {
        public bool? CachedValue { get; init; }

        public bool ThrowOnGet { get; init; }

        public bool ThrowOnSet { get; init; }

        public Guid? WrittenCustomerId { get; private set; }

        public bool? WrittenValue { get; private set; }

        public TimeSpan? WrittenExpiration { get; private set; }

        public Task<bool?> GetAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            if (ThrowOnGet)
            {
                throw new InvalidOperationException("Cache read failed.");
            }

            return Task.FromResult(CachedValue);
        }

        public Task SetAsync(
            Guid customerId,
            bool exists,
            TimeSpan expiration,
            CancellationToken cancellationToken = default)
        {
            if (ThrowOnSet)
            {
                throw new InvalidOperationException("Cache write failed.");
            }

            WrittenCustomerId = customerId;
            WrittenValue = exists;
            WrittenExpiration = expiration;
            return Task.CompletedTask;
        }

        public Task RemoveAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}

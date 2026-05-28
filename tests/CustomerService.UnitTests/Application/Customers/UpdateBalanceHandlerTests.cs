using CustomerService.Application.Customers.Exists;
using CustomerService.Application.Common.Exceptions;
using CustomerService.Application.Customers.UpdateBalance;
using CustomerService.Domain.Entities;
using CustomerService.UnitTests.Application;
using FluentAssertions;

namespace CustomerService.UnitTests.Application.Customers;

public sealed class UpdateBalanceHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidRequest_ShouldTransferBalance()
    {
        var sender = CreateCustomer(balance: 150m);
        var receiver = CreateCustomer(balance: 25m);
        var repository = new FakeCustomerBalanceTransferRepository(sender, receiver);
        var handler = CreateHandler(repository);

        var response = await handler.HandleAsync(sender.Id, new UpdateBalanceRequest(receiver.Id, 40m));

        response.SenderId.Should().Be(sender.Id);
        response.ReceiverId.Should().Be(receiver.Id);
        response.Amount.Should().Be(40m);
        response.SenderBalance.Should().Be(110m);
        sender.BankingDetails.Balance.Should().Be(110m);
        receiver.BankingDetails.Balance.Should().Be(65m);
        repository.TransferCallCount.Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidRequest_ShouldThrowValidationException()
    {
        var sender = CreateCustomer(balance: 150m);
        var repository = new FakeCustomerBalanceTransferRepository(sender);
        var handler = CreateHandler(repository);

        var act = () => handler.HandleAsync(sender.Id, new UpdateBalanceRequest(Guid.Empty, 0m));

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
        repository.TransferCallCount.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_WithSelfTransfer_ShouldThrowInvalidBalanceTransferException()
    {
        var sender = CreateCustomer(balance: 150m);
        var repository = new FakeCustomerBalanceTransferRepository(sender);
        var handler = CreateHandler(repository);

        var act = () => handler.HandleAsync(sender.Id, new UpdateBalanceRequest(sender.Id, 10m));

        await act.Should().ThrowAsync<InvalidBalanceTransferException>();
        repository.TransferCallCount.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_WithMissingSender_ShouldThrowCustomerNotFoundException()
    {
        var receiver = CreateCustomer(balance: 25m);
        var senderId = Guid.NewGuid();
        var repository = new FakeCustomerBalanceTransferRepository(receiver);
        var handler = CreateHandler(repository);

        var act = () => handler.HandleAsync(senderId, new UpdateBalanceRequest(receiver.Id, 10m));

        var exception = await act.Should().ThrowAsync<CustomerNotFoundException>();
        exception.Which.CustomerId.Should().Be(senderId);
    }

    [Fact]
    public async Task HandleAsync_WithMissingReceiver_ShouldThrowCustomerNotFoundException()
    {
        var sender = CreateCustomer(balance: 150m);
        var receiverId = Guid.NewGuid();
        var repository = new FakeCustomerBalanceTransferRepository(sender);
        var handler = CreateHandler(repository);

        var act = () => handler.HandleAsync(sender.Id, new UpdateBalanceRequest(receiverId, 10m));

        var exception = await act.Should().ThrowAsync<CustomerNotFoundException>();
        exception.Which.CustomerId.Should().Be(receiverId);
    }

    [Fact]
    public async Task HandleAsync_WithInsufficientBalance_ShouldThrowInsufficientBalanceException()
    {
        var sender = CreateCustomer(balance: 9m);
        var receiver = CreateCustomer(balance: 25m);
        var repository = new FakeCustomerBalanceTransferRepository(sender, receiver);
        var handler = CreateHandler(repository);

        var act = () => handler.HandleAsync(sender.Id, new UpdateBalanceRequest(receiver.Id, 10m));

        var exception = await act.Should().ThrowAsync<InsufficientBalanceException>();
        exception.Which.CustomerId.Should().Be(sender.Id);
        sender.BankingDetails.Balance.Should().Be(9m);
        receiver.BankingDetails.Balance.Should().Be(25m);
    }

    [Fact]
    public async Task HandleAsync_WhenPersistenceFails_ShouldPropagateFailure()
    {
        var sender = CreateCustomer(balance: 150m);
        var receiver = CreateCustomer(balance: 25m);
        var repository = new FakeCustomerBalanceTransferRepository(sender, receiver)
        {
            ThrowAfterDebit = true
        };
        var handler = CreateHandler(repository);

        var act = () => handler.HandleAsync(sender.Id, new UpdateBalanceRequest(receiver.Id, 10m));

        await act.Should().ThrowAsync<InvalidOperationException>();
        receiver.BankingDetails.Balance.Should().Be(25m);
    }

    [Fact]
    public async Task HandleAsync_WhenTransferSucceeds_ShouldRefreshSenderAndReceiverDetailsCache()
    {
        var sender = CreateCustomer(balance: 150m);
        var receiver = CreateCustomer(balance: 25m);
        var repository = new FakeCustomerBalanceTransferRepository(sender, receiver);
        var cache = new RecordingCustomerDetailsCache();
        var handler = CreateHandler(repository, cache);

        await handler.HandleAsync(sender.Id, new UpdateBalanceRequest(receiver.Id, 40m));

        cache.Writes.Should().HaveCount(2);
        cache.Writes.Should().Contain(write =>
            write.CustomerId == sender.Id &&
            write.Value.BankingDetails.Balance == 110m &&
            write.Expiration == TimeSpan.FromSeconds(123));
        cache.Writes.Should().Contain(write =>
            write.CustomerId == receiver.Id &&
            write.Value.BankingDetails.Balance == 65m &&
            write.Expiration == TimeSpan.FromSeconds(123));
    }

    [Fact]
    public async Task HandleAsync_WhenCacheRefreshFails_ShouldReturnSuccessfulResponse()
    {
        var sender = CreateCustomer(balance: 150m);
        var receiver = CreateCustomer(balance: 25m);
        var repository = new FakeCustomerBalanceTransferRepository(sender, receiver);
        var cache = new RecordingCustomerDetailsCache { ThrowOnSet = true };
        var handler = CreateHandler(repository, cache);

        var response = await handler.HandleAsync(sender.Id, new UpdateBalanceRequest(receiver.Id, 40m));

        response.SenderId.Should().Be(sender.Id);
        response.ReceiverId.Should().Be(receiver.Id);
        response.SenderBalance.Should().Be(110m);
    }

    private static Customer CreateCustomer(decimal balance)
    {
        return new Customer(
            "Maria Silva",
            $"{Guid.NewGuid():N}@example.com",
            "Rua A, 123",
            new BankingDetails("0001", Guid.NewGuid().ToString("N")[..8], balance));
    }

    private static UpdateBalanceHandler CreateHandler(
        FakeCustomerBalanceTransferRepository repository,
        CustomerService.Application.Abstractions.ICustomerDetailsCache? cache = null)
    {
        return new UpdateBalanceHandler(
            repository,
            cache ?? new FakeCustomerDetailsCache(),
            new CustomerExistenceCacheOptions { DetailsTtlSeconds = 123 },
            new UpdateBalanceRequestValidator());
    }

    private sealed class RecordingCustomerDetailsCache : CustomerService.Application.Abstractions.ICustomerDetailsCache
    {
        public bool ThrowOnSet { get; init; }

        public List<(Guid CustomerId, CustomerService.Application.Customers.GetCustomerDetails.CustomerDetailsResponse Value, TimeSpan Expiration)> Writes { get; } = [];

        public Task<CustomerService.Application.Customers.GetCustomerDetails.CustomerDetailsResponse?> GetAsync(
            Guid customerId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<CustomerService.Application.Customers.GetCustomerDetails.CustomerDetailsResponse?>(null);
        }

        public Task SetAsync(
            Guid customerId,
            CustomerService.Application.Customers.GetCustomerDetails.CustomerDetailsResponse details,
            TimeSpan expiration,
            CancellationToken cancellationToken = default)
        {
            if (ThrowOnSet)
            {
                throw new InvalidOperationException("Cache write failed.");
            }

            Writes.Add((customerId, details, expiration));
            return Task.CompletedTask;
        }

        public Task RemoveAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}

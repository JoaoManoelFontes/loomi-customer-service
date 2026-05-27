using CustomerService.Application.Abstractions;
using CustomerService.Application.Common.Exceptions;
using CustomerService.Domain.Entities;

namespace CustomerService.UnitTests.Application;

internal sealed class FakeCustomerBalanceTransferRepository(params Customer[] customers) : ICustomerBalanceTransferRepository
{
    private readonly List<Customer> _customers = [.. customers];

    public int TransferCallCount { get; private set; }

    public bool ThrowAfterDebit { get; init; }

    public Task<CustomerBalanceTransferResult> TransferAsync(
        Guid senderId,
        Guid receiverId,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        TransferCallCount++;

        var sender = _customers.FirstOrDefault(customer => customer.Id == senderId)
            ?? throw new CustomerNotFoundException(senderId);
        var receiver = _customers.FirstOrDefault(customer => customer.Id == receiverId)
            ?? throw new CustomerNotFoundException(receiverId);

        if (sender.BankingDetails.Balance < amount)
        {
            throw new InsufficientBalanceException(senderId);
        }

        sender.BankingDetails.Debit(amount);

        if (ThrowAfterDebit)
        {
            throw new InvalidOperationException("Simulated persistence failure.");
        }

        receiver.BankingDetails.Credit(amount);

        return Task.FromResult(new CustomerBalanceTransferResult(sender, receiver, amount));
    }
}

using CustomerService.Domain.Entities;

namespace CustomerService.Application.Abstractions;

public interface ICustomerBalanceTransferRepository
{
    Task<CustomerBalanceTransferResult> TransferAsync(
        Guid senderId,
        Guid receiverId,
        decimal amount,
        CancellationToken cancellationToken = default);
}

public sealed record CustomerBalanceTransferResult(
    Customer Sender,
    Customer Receiver,
    decimal Amount);

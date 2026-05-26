using CustomerService.Application.Abstractions;
using CustomerService.Application.Common.Exceptions;
using CustomerService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.Infrastructure.Persistence.Repositories;

public sealed class CustomerBalanceTransferRepository(CustomerDbContext dbContext) : ICustomerBalanceTransferRepository
{
    public async Task<CustomerBalanceTransferResult> TransferAsync(
        Guid senderId,
        Guid receiverId,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var customers = await LoadCustomersForUpdateAsync(senderId, receiverId, cancellationToken);
        var sender = customers.FirstOrDefault(customer => customer.Id == senderId)
            ?? throw new CustomerNotFoundException(senderId);
        var receiver = customers.FirstOrDefault(customer => customer.Id == receiverId)
            ?? throw new CustomerNotFoundException(receiverId);

        if (sender.BankingDetails.Balance < amount)
        {
            throw new InsufficientBalanceException(senderId);
        }

        sender.BankingDetails.Debit(amount);
        receiver.BankingDetails.Credit(amount);

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new CustomerBalanceTransferResult(sender, receiver, amount);
    }

    private async Task<List<Customer>> LoadCustomersForUpdateAsync(
        Guid senderId,
        Guid receiverId,
        CancellationToken cancellationToken)
    {
        var orderedIds = new[] { senderId, receiverId }
            .Order()
            .ToArray();

        await dbContext.BankingDetails
            .FromSqlInterpolated($"""
                SELECT *
                FROM banking_details
                WHERE customer_id = {orderedIds[0]} OR customer_id = {orderedIds[1]}
                ORDER BY customer_id
                FOR UPDATE
                """)
            .ToListAsync(cancellationToken);

        return await dbContext.Customers
            .Include(customer => customer.BankingDetails)
            .Where(customer => orderedIds.Contains(customer.Id))
            .OrderBy(customer => customer.Id)
            .ToListAsync(cancellationToken);
    }
}

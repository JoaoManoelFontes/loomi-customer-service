using CustomerService.Application.Abstractions;
using CustomerService.Application.Common.Exceptions;
using CustomerService.Application.Customers.Exists;
using CustomerService.Application.Customers.GetCustomerDetails;
using FluentValidation;

namespace CustomerService.Application.Customers.UpdateBalance;

public sealed class UpdateBalanceHandler(
    ICustomerBalanceTransferRepository balanceTransferRepository,
    ICustomerDetailsCache customerDetailsCache,
    CustomerExistenceCacheOptions cacheOptions,
    IValidator<UpdateBalanceRequest> validator)
{
    public async Task<UpdateBalanceResponse> HandleAsync(
        Guid senderId,
        UpdateBalanceRequest request,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        if (senderId == request.ReceiverId)
        {
            throw new InvalidBalanceTransferException("Sender and receiver must be different customers.");
        }

        var result = await balanceTransferRepository.TransferAsync(
            senderId,
            request.ReceiverId,
            request.Amount,
            cancellationToken);

        await UpdateCustomerDetailsCacheAsync(result.Sender, cancellationToken);
        await UpdateCustomerDetailsCacheAsync(result.Receiver, cancellationToken);

        return new UpdateBalanceResponse(
            result.Sender.Id,
            result.Receiver.Id,
            result.Amount,
            result.Sender.BankingDetails.Balance);
    }

    private async Task UpdateCustomerDetailsCacheAsync(
        Domain.Entities.Customer customer,
        CancellationToken cancellationToken)
    {
        try
        {
            await customerDetailsCache.SetAsync(
                customer.Id,
                customer.ToDetailsResponse(),
                cacheOptions.DetailsTtl,
                cancellationToken);
        }
        catch
        {
            // Cache synchronization must not turn a successful balance update into an API failure.
        }
    }
}

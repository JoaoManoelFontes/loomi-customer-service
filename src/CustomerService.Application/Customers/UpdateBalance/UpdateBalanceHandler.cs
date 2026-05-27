using CustomerService.Application.Abstractions;
using CustomerService.Application.Common.Exceptions;
using FluentValidation;

namespace CustomerService.Application.Customers.UpdateBalance;

public sealed class UpdateBalanceHandler(
    ICustomerBalanceTransferRepository balanceTransferRepository,
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

        return new UpdateBalanceResponse(
            result.Sender.Id,
            result.Receiver.Id,
            result.Amount,
            result.Sender.BankingDetails.Balance);
    }
}

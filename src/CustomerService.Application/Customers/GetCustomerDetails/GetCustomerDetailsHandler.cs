using CustomerService.Application.Abstractions;
using CustomerService.Application.Common.Exceptions;

namespace CustomerService.Application.Customers.GetCustomerDetails;

public sealed class GetCustomerDetailsHandler(ICustomerRepository customers)
{
    public async Task<CustomerDetailsResponse> HandleAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        var customer = await customers.GetByIdAsync(customerId, cancellationToken)
            ?? throw new CustomerNotFoundException(customerId);

        return new CustomerDetailsResponse(
            customer.Id,
            customer.Name,
            customer.Email,
            customer.Address,
            customer.ProfilePictureUrl,
            new CustomerBankingDetailsResponse(
                customer.BankingDetails.Agency,
                customer.BankingDetails.CheckingAccountNumber,
                customer.BankingDetails.Balance));
    }
}

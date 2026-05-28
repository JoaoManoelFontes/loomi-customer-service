using CustomerService.Domain.Entities;

namespace CustomerService.Application.Customers.GetCustomerDetails;

internal static class CustomerDetailsMapper
{
    public static CustomerDetailsResponse ToDetailsResponse(this Customer customer)
    {
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

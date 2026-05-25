namespace CustomerService.Application.Customers.GetCustomerDetails;

public sealed record CustomerBankingDetailsResponse(
    string Agency,
    string CheckingAccountNumber,
    decimal Balance);

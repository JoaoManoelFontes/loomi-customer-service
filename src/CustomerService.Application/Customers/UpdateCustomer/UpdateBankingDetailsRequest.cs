namespace CustomerService.Application.Customers.UpdateCustomer;

public sealed record UpdateBankingDetailsRequest(
    string? Agency,
    string? CheckingAccountNumber);

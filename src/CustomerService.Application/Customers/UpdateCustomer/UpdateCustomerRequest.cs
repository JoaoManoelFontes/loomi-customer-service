namespace CustomerService.Application.Customers.UpdateCustomer;

public sealed record UpdateCustomerRequest(
    string? Name,
    string? Email,
    string? Address,
    UpdateBankingDetailsRequest? BankingDetails);

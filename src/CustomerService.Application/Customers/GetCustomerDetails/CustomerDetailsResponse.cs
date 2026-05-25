namespace CustomerService.Application.Customers.GetCustomerDetails;

public sealed record CustomerDetailsResponse(
    Guid Id,
    string Name,
    string Email,
    string Address,
    string? ProfilePictureUrl,
    CustomerBankingDetailsResponse BankingDetails);

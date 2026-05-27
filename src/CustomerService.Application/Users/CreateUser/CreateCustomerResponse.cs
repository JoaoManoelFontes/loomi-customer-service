namespace CustomerService.Application.Users.CreateUser;

public sealed record CreateCustomerResponse(
    Guid? Id,
    string Name,
    string Email,
    string Address,
    string Agency,
    string CheckingAccountNumber,
    decimal Balance
    );

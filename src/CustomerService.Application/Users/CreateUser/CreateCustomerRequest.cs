namespace CustomerService.Application.Users.CreateUser;

public sealed record CreateCustomerRequest(
    string Cpf,
    string Password,
    string Role,
    string Name,
    string Email,
    string Address,
    string Agency,
    string CheckingAccountNumber,
    decimal Balance);

using CustomerService.Domain.Enums;

namespace CustomerService.Application.Users.CreateUser;

public sealed record CreateUserResponse(
    Guid Id,
    string Cpf,
    UserRole Role,
    CreateCustomerResponse Customer
    );

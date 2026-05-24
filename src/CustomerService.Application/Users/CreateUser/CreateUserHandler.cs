using CustomerService.Application.Abstractions;
using CustomerService.Application.Common;
using CustomerService.Application.Common.Exceptions;
using CustomerService.Domain.Entities;
using CustomerService.Domain.Enums;
using FluentValidation;

namespace CustomerService.Application.Users.CreateUser;

public sealed class CreateUserHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IValidator<CreateCustomerRequest> validator)
{
    public async Task<CreateUserResponse> HandleAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var cpf = CpfNormalizer.Normalize(request.Cpf);

        if (await users.ExistsByCpfAsync(cpf, cancellationToken))
        {
            throw new DuplicateCpfException(cpf);
        }

        var role = Enum.Parse<UserRole>(request.Role, ignoreCase: true);
        var bankingDetails = new BankingDetails(request.Agency, request.CheckingAccountNumber, balance: 0);
        var customer = new Customer(request.Name, request.Email, request.Address, bankingDetails);
        var user = new User(cpf, passwordHasher.Hash(request.Password), role, customer.Id);

        await users.AddAsync(customer, user, cancellationToken);

        var customerResponse = new CreateCustomerResponse(
            customer.Id,
            customer.Name,
            customer.Email,
            customer.Address,
            customer.BankingDetails.Agency,
            customer.BankingDetails.CheckingAccountNumber
        );

        return new CreateUserResponse(
            user.Id,
            user.Cpf,
            user.Role,
            customerResponse
            );
    }
}

using CustomerService.Application.Abstractions;
using CustomerService.Domain.Entities;

namespace CustomerService.UnitTests.Application;

internal sealed class FakeUserRepository(params User[] users) : IUserRepository
{
    private readonly List<User> _users = [.. users];

    public User? AddedUser { get; private set; }

    public Customer? AddedCustomer { get; private set; }

    public Task<User?> GetByCpfAsync(string cpf, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_users.FirstOrDefault(user => user.Cpf == cpf));
    }

    public Task<bool> ExistsByCpfAsync(string cpf, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_users.Any(user => user.Cpf == cpf));
    }

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        AddedUser = user;
        _users.Add(user);

        return Task.CompletedTask;
    }

    public Task AddAsync(Customer customer, User user, CancellationToken cancellationToken = default)
    {
        AddedCustomer = customer;
        AddedUser = user;
        _users.Add(user);

        return Task.CompletedTask;
    }
}

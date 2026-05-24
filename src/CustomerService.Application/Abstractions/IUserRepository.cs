using CustomerService.Domain.Entities;

namespace CustomerService.Application.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByCpfAsync(string cpf, CancellationToken cancellationToken = default);

    Task<bool> ExistsByCpfAsync(string cpf, CancellationToken cancellationToken = default);

    Task AddAsync(User user, CancellationToken cancellationToken = default);

    Task AddAsync(Customer customer, User user, CancellationToken cancellationToken = default);
}

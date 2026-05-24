using CustomerService.Application.Abstractions;
using CustomerService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(CustomerDbContext dbContext) : IUserRepository
{
    public Task<User?> GetByCpfAsync(string cpf, CancellationToken cancellationToken = default)
    {
        return dbContext.Users.FirstOrDefaultAsync(user => user.Cpf == cpf, cancellationToken);
    }

    public Task<bool> ExistsByCpfAsync(string cpf, CancellationToken cancellationToken = default)
    {
        return dbContext.Users.AnyAsync(user => user.Cpf == cpf, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await dbContext.Users.AddAsync(user, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddAsync(Customer customer, User user, CancellationToken cancellationToken = default)
    {
        await dbContext.Customers.AddAsync(customer, cancellationToken);
        await dbContext.Users.AddAsync(user, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

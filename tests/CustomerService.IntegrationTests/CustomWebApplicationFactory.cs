using CustomerService.Application.Abstractions;
using CustomerService.Application.Common;
using CustomerService.Domain.Entities;
using CustomerService.Domain.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CustomerService.IntegrationTests;

internal sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly TestUserRepository _users = new();
    private readonly TestCustomerRepository _customers = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration(configuration =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = "Host=localhost;Database=test;Username=test;Password=test",
                ["Jwt:Issuer"] = "CustomerService",
                ["Jwt:Audience"] = "BankingSystem",
                ["Jwt:Secret"] = "change-this-development-secret-at-least-32-chars",
                ["Jwt:ExpiresInMinutes"] = "60"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IUserRepository>();
            services.RemoveAll<ICustomerRepository>();
            services.AddSingleton<IUserRepository>(_users);
            services.AddSingleton<ICustomerRepository>(_customers);
        });
    }

    public async Task SeedUserAsync(string cpf, string password, UserRole role, Guid? customerId = null)
    {
        using var scope = Services.CreateScope();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        await _users.AddAsync(new User(CpfNormalizer.Normalize(cpf), passwordHasher.Hash(password), role, customerId));
    }

    public async Task<Customer> SeedCustomerAsync(
        string name = "Maria Silva",
        string email = "maria.silva@example.com",
        string address = "Rua A, 123",
        string agency = "0001",
        string checkingAccountNumber = "123456-7",
        decimal balance = 100)
    {
        var customer = new Customer(
            name,
            email,
            address,
            new BankingDetails(agency, checkingAccountNumber, balance));

        await _customers.AddAsync(customer);
        return customer;
    }

    private sealed class TestUserRepository : IUserRepository
    {
        private readonly List<User> _users = [];

        public Task<User?> GetByCpfAsync(string cpf, CancellationToken cancellationToken = default)
        {
            lock (_users)
            {
                return Task.FromResult(_users.FirstOrDefault(user => user.Cpf == cpf));
            }
        }

        public Task<bool> ExistsByCpfAsync(string cpf, CancellationToken cancellationToken = default)
        {
            lock (_users)
            {
                return Task.FromResult(_users.Any(user => user.Cpf == cpf));
            }
        }

        public Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            lock (_users)
            {
                _users.Add(user);
            }

            return Task.CompletedTask;
        }

        public Task AddAsync(Customer customer, User user, CancellationToken cancellationToken = default)
        {
            lock (_users)
            {
                _users.Add(user);
            }

            return Task.CompletedTask;
        }
    }

    private sealed class TestCustomerRepository : ICustomerRepository
    {
        private readonly List<Customer> _customers = [];

        public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            lock (_customers)
            {
                return Task.FromResult(_customers.FirstOrDefault(customer => customer.Id == id));
            }
        }

        public Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default)
        {
            lock (_customers)
            {
                var index = _customers.FindIndex(existing => existing.Id == customer.Id);
                if (index >= 0)
                {
                    _customers[index] = customer;
                }
                else
                {
                    _customers.Add(customer);
                }
            }

            return Task.CompletedTask;
        }

        public Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
        {
            lock (_customers)
            {
                _customers.Add(customer);
            }

            return Task.CompletedTask;
        }
    }
}

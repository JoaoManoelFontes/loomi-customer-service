using CustomerService.Application.Abstractions;
using CustomerService.Domain.Entities;

namespace CustomerService.UnitTests.Application;

internal sealed class FakeCustomerRepository(params Customer[] customers) : ICustomerRepository
{
    private readonly List<Customer> _customers = [.. customers];

    public Customer? UpdatedCustomer { get; private set; }

    public int GetByIdCallCount { get; private set; }

    public int ExistsCallCount { get; private set; }

    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        GetByIdCallCount++;
        return Task.FromResult(_customers.FirstOrDefault(customer => customer.Id == id));
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        ExistsCallCount++;
        return Task.FromResult(_customers.Any(customer => customer.Id == id));
    }

    public Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        UpdatedCustomer = customer;
        return Task.CompletedTask;
    }
}

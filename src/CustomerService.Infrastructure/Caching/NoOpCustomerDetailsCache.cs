using CustomerService.Application.Abstractions;
using CustomerService.Application.Customers.GetCustomerDetails;

namespace CustomerService.Infrastructure.Caching;

internal sealed class NoOpCustomerDetailsCache : ICustomerDetailsCache
{
    public Task<CustomerDetailsResponse?> GetAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<CustomerDetailsResponse?>(null);
    }

    public Task SetAsync(
        Guid customerId,
        CustomerDetailsResponse details,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task RemoveAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

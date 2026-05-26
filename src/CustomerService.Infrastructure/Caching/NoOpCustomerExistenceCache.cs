using CustomerService.Application.Abstractions;

namespace CustomerService.Infrastructure.Caching;

internal sealed class NoOpCustomerExistenceCache : ICustomerExistenceCache
{
    public Task<bool?> GetAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<bool?>(null);
    }

    public Task SetAsync(
        Guid customerId,
        bool exists,
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

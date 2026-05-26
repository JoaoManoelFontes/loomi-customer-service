using CustomerService.Application.Abstractions;

namespace CustomerService.Application.Customers.Exists;

public sealed class CustomerExistsHandler(
    ICustomerRepository customers,
    ICustomerExistenceCache cache,
    CustomerExistenceCacheOptions cacheOptions)
{
    public async Task<bool> HandleAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cached = await cache.GetAsync(customerId, cancellationToken);
            if (cached.HasValue)
            {
                return cached.Value;
            }
        }
        catch
        {
            // Redis is an optimization; PostgreSQL remains the source of truth.
        }

        var exists = await customers.ExistsAsync(customerId, cancellationToken);

        try
        {
            await cache.SetAsync(customerId, exists, cacheOptions.ExistsTtl, cancellationToken);
        }
        catch
        {
            // Ignore cache write failures so the endpoint contract is preserved.
        }

        return exists;
    }
}

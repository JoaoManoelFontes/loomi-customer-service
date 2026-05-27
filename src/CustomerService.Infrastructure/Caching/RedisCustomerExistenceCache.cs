using CustomerService.Application.Abstractions;
using CustomerService.Application.Customers.Exists;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace CustomerService.Infrastructure.Caching;

public sealed class RedisCustomerExistenceCache(
    IConnectionMultiplexer connection,
    ILogger<RedisCustomerExistenceCache> logger) : ICustomerExistenceCache
{
    public async Task<bool?> GetAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var database = connection.GetDatabase();
            var value = await database.StringGetAsync(CustomerCacheKeys.Exists(customerId));

            if (!value.HasValue)
            {
                return null;
            }

            return bool.TryParse(value.ToString(), out var exists) ? exists : null;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Failed to read customer existence cache for customer {CustomerId}.", customerId);
            return null;
        }
    }

    public async Task SetAsync(
        Guid customerId,
        bool exists,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var database = connection.GetDatabase();
            await database.StringSetAsync(CustomerCacheKeys.Exists(customerId), exists.ToString(), expiration);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Failed to write customer existence cache for customer {CustomerId}.", customerId);
        }
    }

    public async Task RemoveAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var database = connection.GetDatabase();
            await database.KeyDeleteAsync(CustomerCacheKeys.Exists(customerId));
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Failed to remove customer existence cache for customer {CustomerId}.", customerId);
        }
    }
}

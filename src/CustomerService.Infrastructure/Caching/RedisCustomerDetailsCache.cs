using System.Text.Json;
using CustomerService.Application.Abstractions;
using CustomerService.Application.Customers.GetCustomerDetails;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace CustomerService.Infrastructure.Caching;

public sealed class RedisCustomerDetailsCache(
    IConnectionMultiplexer connection,
    ILogger<RedisCustomerDetailsCache> logger) : ICustomerDetailsCache
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<CustomerDetailsResponse?> GetAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var database = connection.GetDatabase();
            var value = await database.StringGetAsync(CustomerCacheKeys.Details(customerId));

            if (!value.HasValue)
            {
                return null;
            }

            return JsonSerializer.Deserialize<CustomerDetailsResponse>(value.ToString(), JsonOptions);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Failed to read customer details cache for customer {CustomerId}.", customerId);
            return null;
        }
    }

    public async Task SetAsync(
        Guid customerId,
        CustomerDetailsResponse details,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var database = connection.GetDatabase();
            var value = JsonSerializer.Serialize(details, JsonOptions);
            await database.StringSetAsync(CustomerCacheKeys.Details(customerId), value, expiration);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Failed to write customer details cache for customer {CustomerId}.", customerId);
        }
    }

    public async Task RemoveAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var database = connection.GetDatabase();
            await database.KeyDeleteAsync(CustomerCacheKeys.Details(customerId));
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Failed to remove customer details cache for customer {CustomerId}.", customerId);
        }
    }
}

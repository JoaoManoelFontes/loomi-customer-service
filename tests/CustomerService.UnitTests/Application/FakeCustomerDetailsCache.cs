using CustomerService.Application.Abstractions;
using CustomerService.Application.Customers.GetCustomerDetails;

namespace CustomerService.UnitTests.Application;

internal sealed class FakeCustomerDetailsCache : ICustomerDetailsCache
{
    public CustomerDetailsResponse? CachedValue { get; init; }

    public bool ThrowOnGet { get; init; }

    public bool ThrowOnSet { get; init; }

    public Guid? WrittenCustomerId { get; private set; }

    public CustomerDetailsResponse? WrittenValue { get; private set; }

    public TimeSpan? WrittenExpiration { get; private set; }

    public Task<CustomerDetailsResponse?> GetAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        if (ThrowOnGet)
        {
            throw new InvalidOperationException("Cache read failed.");
        }

        return Task.FromResult(CachedValue);
    }

    public Task SetAsync(
        Guid customerId,
        CustomerDetailsResponse details,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        if (ThrowOnSet)
        {
            throw new InvalidOperationException("Cache write failed.");
        }

        WrittenCustomerId = customerId;
        WrittenValue = details;
        WrittenExpiration = expiration;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

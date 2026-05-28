using CustomerService.Application.Abstractions;
using CustomerService.Application.Common.Exceptions;
using CustomerService.Application.Customers.Exists;

namespace CustomerService.Application.Customers.GetCustomerDetails;

public sealed class GetCustomerDetailsHandler(
    ICustomerRepository customers,
    ICustomerDetailsCache cache,
    CustomerExistenceCacheOptions cacheOptions)
{
    public async Task<CustomerDetailsResponse> HandleAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cached = await cache.GetAsync(customerId, cancellationToken);
            if (cached is not null)
            {
                return cached;
            }
        }
        catch
        {
            // Redis is an optimization; PostgreSQL remains the source of truth.
        }

        var customer = await customers.GetByIdAsync(customerId, cancellationToken)
            ?? throw new CustomerNotFoundException(customerId);

        var response = customer.ToDetailsResponse();

        try
        {
            await cache.SetAsync(customerId, response, cacheOptions.DetailsTtl, cancellationToken);
        }
        catch
        {
            // Ignore cache write failures so the endpoint contract is preserved.
        }

        return response;
    }
}

using CustomerService.Application.Customers.GetCustomerDetails;

namespace CustomerService.Application.Abstractions;

public interface ICustomerDetailsCache
{
    Task<CustomerDetailsResponse?> GetAsync(Guid customerId, CancellationToken cancellationToken = default);

    Task SetAsync(
        Guid customerId,
        CustomerDetailsResponse details,
        TimeSpan expiration,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(Guid customerId, CancellationToken cancellationToken = default);
}

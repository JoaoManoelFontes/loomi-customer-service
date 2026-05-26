using CustomerService.Application.Abstractions;

namespace CustomerService.Application.Customers.Exists;

public sealed class CustomerExistsHandler(ICustomerRepository customers)
{
    public Task<bool> HandleAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        return customers.ExistsAsync(customerId, cancellationToken);
    }
}

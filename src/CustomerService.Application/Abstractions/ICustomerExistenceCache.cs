namespace CustomerService.Application.Abstractions;

public interface ICustomerExistenceCache
{
    Task<bool?> GetAsync(Guid customerId, CancellationToken cancellationToken = default);

    Task SetAsync(Guid customerId, bool exists, TimeSpan expiration, CancellationToken cancellationToken = default);

    Task RemoveAsync(Guid customerId, CancellationToken cancellationToken = default);
}

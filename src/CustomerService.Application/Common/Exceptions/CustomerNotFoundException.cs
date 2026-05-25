namespace CustomerService.Application.Common.Exceptions;

public sealed class CustomerNotFoundException(Guid customerId)
    : Exception($"Customer '{customerId}' was not found.")
{
    public Guid CustomerId { get; } = customerId;
}

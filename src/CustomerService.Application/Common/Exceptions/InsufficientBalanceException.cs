namespace CustomerService.Application.Common.Exceptions;

public sealed class InsufficientBalanceException(Guid customerId)
    : Exception($"Customer '{customerId}' has insufficient balance.")
{
    public Guid CustomerId { get; } = customerId;
}

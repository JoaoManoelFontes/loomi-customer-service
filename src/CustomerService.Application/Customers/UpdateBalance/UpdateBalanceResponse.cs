namespace CustomerService.Application.Customers.UpdateBalance;

public sealed record UpdateBalanceResponse(
    Guid SenderId,
    Guid ReceiverId,
    decimal Amount,
    decimal SenderBalance);

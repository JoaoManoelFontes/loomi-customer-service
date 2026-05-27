namespace CustomerService.Application.Customers.UpdateBalance;

public sealed record UpdateBalanceRequest(Guid ReceiverId, decimal Amount);

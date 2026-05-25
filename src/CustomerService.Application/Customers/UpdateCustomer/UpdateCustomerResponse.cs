namespace CustomerService.Application.Customers.UpdateCustomer;

public sealed record UpdateCustomerResponse(
    Guid CustomerId,
    string Status);

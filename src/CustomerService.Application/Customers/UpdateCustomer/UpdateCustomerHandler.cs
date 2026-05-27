using CustomerService.Application.Abstractions;
using CustomerService.Application.Common.Exceptions;
using FluentValidation;

namespace CustomerService.Application.Customers.UpdateCustomer;

public sealed class UpdateCustomerHandler(
    ICustomerRepository customers,
    IValidator<UpdateCustomerRequest> validator)
{
    public async Task<UpdateCustomerResponse> HandleAsync(
        Guid customerId,
        UpdateCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var customer = await customers.GetByIdAsync(customerId, cancellationToken)
            ?? throw new CustomerNotFoundException(customerId);

        customer.UpdateProfileFields(request.Name, request.Email, request.Address);
        if (request.ProfileImageUrl is not null)
        {
            customer.UpdateProfilePicture(request.ProfileImageUrl);
        }

        customer.UpdateBankingDetails(
            request.BankingDetails?.Agency,
            request.BankingDetails?.CheckingAccountNumber);

        await customers.UpdateAsync(customer, cancellationToken);

        return new UpdateCustomerResponse(customer.Id, "updated");
    }
}

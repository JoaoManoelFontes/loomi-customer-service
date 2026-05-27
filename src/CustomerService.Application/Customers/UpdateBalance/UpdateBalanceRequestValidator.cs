using FluentValidation;

namespace CustomerService.Application.Customers.UpdateBalance;

public sealed class UpdateBalanceRequestValidator : AbstractValidator<UpdateBalanceRequest>
{
    public UpdateBalanceRequestValidator()
    {
        RuleFor(request => request.ReceiverId)
            .NotEmpty();

        RuleFor(request => request.Amount)
            .GreaterThan(0);
    }
}

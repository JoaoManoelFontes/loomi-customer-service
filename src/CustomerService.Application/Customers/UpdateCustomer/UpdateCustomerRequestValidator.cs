using FluentValidation;

namespace CustomerService.Application.Customers.UpdateCustomer;

public sealed class UpdateCustomerRequestValidator : AbstractValidator<UpdateCustomerRequest>
{
    public UpdateCustomerRequestValidator()
    {
        RuleFor(request => request)
            .Must(ContainAtLeastOneSupportedField)
            .WithMessage("At least one supported field must be provided.");

        When(request => request.Name is not null, () =>
        {
            RuleFor(request => request.Name)
                .NotEmpty()
                .MaximumLength(150);
        });

        When(request => request.Email is not null, () =>
        {
            RuleFor(request => request.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(254);
        });

        When(request => request.Address is not null, () =>
        {
            RuleFor(request => request.Address)
                .NotEmpty()
                .MaximumLength(500);
        });

        When(request => request.ProfileImageUrl is not null, () =>
        {
            RuleFor(request => request.ProfileImageUrl)
                .NotEmpty()
                .MaximumLength(2048)
                .Must(BeAbsoluteUrl)
                .WithMessage("ProfileImageUrl must be an absolute URL.");
        });

        When(request => request.BankingDetails?.Agency is not null, () =>
        {
            RuleFor(request => request.BankingDetails!.Agency)
                .NotEmpty()
                .MaximumLength(20);
        });

        When(request => request.BankingDetails?.CheckingAccountNumber is not null, () =>
        {
            RuleFor(request => request.BankingDetails!.CheckingAccountNumber)
                .NotEmpty()
                .MaximumLength(30);
        });
    }

    private static bool ContainAtLeastOneSupportedField(UpdateCustomerRequest request)
    {
        return request.Name is not null
            || request.Email is not null
            || request.Address is not null
            || request.ProfileImageUrl is not null
            || request.BankingDetails?.Agency is not null
            || request.BankingDetails?.CheckingAccountNumber is not null;
    }

    private static bool BeAbsoluteUrl(string? value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}

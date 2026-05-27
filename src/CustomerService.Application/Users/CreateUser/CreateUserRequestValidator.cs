using CustomerService.Application.Common;
using CustomerService.Domain.Enums;
using FluentValidation;

namespace CustomerService.Application.Users.CreateUser;

public sealed class CreateUserRequestValidator : AbstractValidator<CreateCustomerRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(request => request.Cpf)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(cpf => CpfNormalizer.Normalize(cpf).Length == 11)
            .WithMessage("Cpf must contain 11 digits.");

        RuleFor(request => request.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MinimumLength(8);

        RuleFor(request => request.Role)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(role => Enum.TryParse<UserRole>(role, ignoreCase: true, out _))
            .WithMessage("Role must be one of: Admin, Customer, Service.");

        RuleFor(request => request.Name)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(request => request.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(254);

        RuleFor(request => request.Address)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(request => request.Agency)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(request => request.CheckingAccountNumber)
            .NotEmpty()
            .MaximumLength(30);

        RuleFor(request => request.Balance)
            .GreaterThanOrEqualTo(0);
    }
}

using FluentValidation;

namespace CustomerService.Application.Auth.Login;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(request => request.Cpf)
            .NotEmpty();

        RuleFor(request => request.Password)
            .NotEmpty();
    }
}

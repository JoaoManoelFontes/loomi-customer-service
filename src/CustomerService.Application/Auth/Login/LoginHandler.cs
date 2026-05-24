using CustomerService.Application.Abstractions;
using CustomerService.Application.Common;
using CustomerService.Application.Common.Exceptions;
using FluentValidation;

namespace CustomerService.Application.Auth.Login;

public sealed class LoginHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator,
    IValidator<LoginRequest> validator)
{
    public async Task<LoginResponse> HandleAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var cpf = CpfNormalizer.Normalize(request.Cpf);
        var user = await users.GetByCpfAsync(cpf, cancellationToken);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new InvalidCredentialsException();
        }

        var token = jwtTokenGenerator.Generate(user);

        return new LoginResponse(token.AccessToken, token.TokenType, token.ExpiresIn);
    }
}

using CustomerService.Application.Abstractions;
using CustomerService.Application.Auth.Login;
using CustomerService.Application.Common.Exceptions;
using CustomerService.Domain.Entities;
using CustomerService.Domain.Enums;
using CustomerService.UnitTests.Application;
using FluentAssertions;

namespace CustomerService.UnitTests.Application.Auth;

public sealed class LoginHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidCredentials_ShouldReturnBearerToken()
    {
        var user = new User("12345678900", "hashed:Admin@123", UserRole.Admin);
        var repository = new FakeUserRepository(user);
        var handler = new LoginHandler(
            repository,
            new FakePasswordHasher(),
            new FakeJwtTokenGenerator(),
            new LoginRequestValidator());

        var response = await handler.HandleAsync(new LoginRequest("123.456.789-00", "Admin@123"));

        response.AccessToken.Should().Be("token-for-12345678900-Admin");
        response.TokenType.Should().Be("Bearer");
        response.ExpiresIn.Should().Be(3600);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidPassword_ShouldThrowInvalidCredentialsException()
    {
        var user = new User("12345678900", "hashed:Admin@123", UserRole.Admin);
        var handler = new LoginHandler(
            new FakeUserRepository(user),
            new FakePasswordHasher(),
            new FakeJwtTokenGenerator(),
            new LoginRequestValidator());

        var act = () => handler.HandleAsync(new LoginRequest("12345678900", "wrong"));

        await act.Should().ThrowAsync<InvalidCredentialsException>()
            .WithMessage("Invalid CPF or password.");
    }

    private sealed class FakeJwtTokenGenerator : IJwtTokenGenerator
    {
        public JwtTokenResult Generate(User user)
        {
            return new JwtTokenResult($"token-for-{user.Cpf}-{user.Role}", "Bearer", 3600);
        }
    }
}

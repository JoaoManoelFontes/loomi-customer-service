using CustomerService.Domain.Entities;

namespace CustomerService.Application.Abstractions;

public interface IJwtTokenGenerator
{
    JwtTokenResult Generate(User user);
}

public sealed record JwtTokenResult(string AccessToken, string TokenType, int ExpiresIn);

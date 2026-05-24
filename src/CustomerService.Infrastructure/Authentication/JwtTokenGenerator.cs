using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CustomerService.Application.Abstractions;
using CustomerService.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CustomerService.Infrastructure.Authentication;

public sealed class JwtTokenGenerator(IOptions<JwtOptions> options) : IJwtTokenGenerator
{
    public JwtTokenResult Generate(User user)
    {
        var jwtOptions = options.Value;
        var expiresAt = DateTime.UtcNow.AddMinutes(jwtOptions.ExpiresInMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new("cpf", user.Cpf),
            new("role", user.Role.ToString())
        };

        if (user.CustomerId.HasValue)
        {
            claims.Add(new Claim("customer_id", user.CustomerId.Value.ToString()));
        }

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret))
            {
                KeyId = "customer-service-signing-key"
            },
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            jwtOptions.Issuer,
            jwtOptions.Audience,
            claims,
            expires: expiresAt,
            signingCredentials: signingCredentials);

        return new JwtTokenResult(
            new JwtSecurityTokenHandler().WriteToken(token),
            "Bearer",
            Convert.ToInt32(TimeSpan.FromMinutes(jwtOptions.ExpiresInMinutes).TotalSeconds));
    }
}

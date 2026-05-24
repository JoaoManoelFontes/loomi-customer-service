using CustomerService.Application.Auth.Login;
using CustomerService.Application.Users.CreateUser;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerService.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
        services.AddScoped<IValidator<CreateCustomerRequest>, CreateUserRequestValidator>();
        services.AddScoped<LoginHandler>();
        services.AddScoped<CreateUserHandler>();

        return services;
    }
}

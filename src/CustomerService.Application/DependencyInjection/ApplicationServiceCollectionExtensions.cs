using CustomerService.Application.Auth.Login;
using CustomerService.Application.Customers.Exists;
using CustomerService.Application.Customers.GetCustomerDetails;
using CustomerService.Application.Customers.UpdateCustomer;
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
        services.AddScoped<IValidator<UpdateCustomerRequest>, UpdateCustomerRequestValidator>();
        services.AddScoped<LoginHandler>();
        services.AddScoped<CustomerExistsHandler>();
        services.AddScoped<GetCustomerDetailsHandler>();
        services.AddScoped<UpdateCustomerHandler>();
        services.AddScoped<CreateUserHandler>();

        return services;
    }
}

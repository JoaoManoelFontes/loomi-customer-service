using CustomerService.Application.Abstractions;
using CustomerService.Application.Customers.Exists;
using CustomerService.Infrastructure.Authentication;
using CustomerService.Infrastructure.Caching;
using CustomerService.Infrastructure.Persistence;
using CustomerService.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace CustomerService.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'Postgres' is not configured.");
        }

        services.AddDbContext<CustomerDbContext>(options =>
        {
            options.UseNpgsql(
                connectionString,
                npgsqlOptions => npgsqlOptions.MigrationsAssembly(typeof(CustomerDbContext).Assembly.FullName));
        });

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.Issuer), "Jwt:Issuer is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Audience), "Jwt:Audience is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Secret), "Jwt:Secret is required.")
            .Validate(options => options.Secret.Length >= 32, "Jwt:Secret must be at least 32 characters.")
            .Validate(options => options.ExpiresInMinutes > 0, "Jwt:ExpiresInMinutes must be greater than zero.")
            .ValidateOnStart();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICustomerBalanceTransferRepository, CustomerBalanceTransferRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton(configuration.GetSection(CustomerExistenceCacheOptions.SectionName)
            .Get<CustomerExistenceCacheOptions>() ?? new CustomerExistenceCacheOptions());

        var redisConnectionString = configuration.GetSection("Redis")["ConnectionString"];
        if (string.IsNullOrWhiteSpace(redisConnectionString))
        {
            services.AddSingleton<ICustomerExistenceCache, NoOpCustomerExistenceCache>();
        }
        else
        {
            services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));
            services.AddSingleton<ICustomerExistenceCache, RedisCustomerExistenceCache>();
        }

        return services;
    }
}

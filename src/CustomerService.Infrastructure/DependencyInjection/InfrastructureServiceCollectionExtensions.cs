using CustomerService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        return services;
    }
}

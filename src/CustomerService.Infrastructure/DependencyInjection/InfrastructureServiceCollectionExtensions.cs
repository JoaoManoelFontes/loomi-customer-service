using CustomerService.Application.Abstractions;
using CustomerService.Application.Customers.Exists;
using CustomerService.Infrastructure.Authentication;
using CustomerService.Infrastructure.Caching;
using CustomerService.Infrastructure.Persistence;
using CustomerService.Infrastructure.Persistence.Repositories;
using CustomerService.Infrastructure.Storage;
using Azure.Storage.Blobs;
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

        var storageOptions = configuration.GetSection(AzureBlobStorageOptions.SectionName)
            .Get<AzureBlobStorageOptions>() ?? new AzureBlobStorageOptions();
        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(storageOptions));
        if (string.IsNullOrWhiteSpace(storageOptions.ConnectionString))
        {
            services.AddSingleton<IProfilePictureStorageService, LocalProfilePictureStorageService>();
        }
        else
        {
            services.AddSingleton(_ => new BlobContainerClient(
                storageOptions.ConnectionString,
                storageOptions.ContainerName));
            services.AddSingleton<IProfilePictureStorageService, AzureBlobProfilePictureStorageService>();
        }

        services.AddSingleton(configuration.GetSection(CustomerExistenceCacheOptions.SectionName)
            .Get<CustomerExistenceCacheOptions>() ?? new CustomerExistenceCacheOptions());

        var redisOptions = GetRedisOptions(configuration);
        if (redisOptions is null)
        {
            services.AddSingleton<ICustomerExistenceCache, NoOpCustomerExistenceCache>();
            services.AddSingleton<ICustomerDetailsCache, NoOpCustomerDetailsCache>();
        }
        else
        {
            services.AddSingleton<IConnectionMultiplexer>(_ =>
            {
                redisOptions.AbortOnConnectFail = false;

                return ConnectionMultiplexer.Connect(redisOptions);
            });
            services.AddSingleton<ICustomerExistenceCache, RedisCustomerExistenceCache>();
            services.AddSingleton<ICustomerDetailsCache, RedisCustomerDetailsCache>();
        }

        return services;
    }

    private static ConfigurationOptions? GetRedisOptions(IConfiguration configuration)
    {
        var redisSection = configuration.GetSection("Redis");
        var host = redisSection["Host"] ?? configuration["REDIS_HOST"];
        var portValue = redisSection["Port"] ?? configuration["REDIS_PORT"];
        if (!string.IsNullOrWhiteSpace(host) && int.TryParse(portValue, out var port))
        {
            var options = new ConfigurationOptions
            {
                Ssl = GetRedisSsl(configuration, port)
            };
            options.EndPoints.Add(host, port);

            var password = redisSection["Password"] ?? configuration["REDIS_PASSWORD"];
            if (!string.IsNullOrWhiteSpace(password))
            {
                options.Password = password;
            }

            return options;
        }

        var connectionString = redisSection["ConnectionString"];
        return string.IsNullOrWhiteSpace(connectionString)
            ? null
            : ConfigurationOptions.Parse(connectionString);
    }

    private static bool GetRedisSsl(IConfiguration configuration, int port)
    {
        var configuredValue = configuration.GetSection("Redis")["Ssl"] ?? configuration["REDIS_SSL"];
        if (bool.TryParse(configuredValue, out var ssl))
        {
            return ssl;
        }

        return port == 6380;
    }
}

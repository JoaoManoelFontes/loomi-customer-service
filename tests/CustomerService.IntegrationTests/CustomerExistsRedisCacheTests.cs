using System.Net;
using System.Net.Http.Json;
using CustomerService.Application.Abstractions;
using CustomerService.Application.Customers.Exists;
using CustomerService.Domain.Entities;
using CustomerService.Infrastructure.Caching;
using CustomerService.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace CustomerService.IntegrationTests;

public sealed class CustomerExistsRedisCacheTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("customer_service_test")
        .WithUsername("customer_service")
        .WithPassword("customer_service_dev_password")
        .Build();

    private readonly RedisContainer _redis = new RedisBuilder("redis:7-alpine")
        .Build();

    private RedisBackedFactory _factory = null!;
    private HttpClient _client = null!;
    private IConnectionMultiplexer _redisConnection = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await _redis.StartAsync();

        var postgresConnectionString =
            $"Host=localhost;Port={_postgres.GetMappedPublicPort(5432)};Database=customer_service_test;Username=customer_service;Password=customer_service_dev_password";
        var redisConnectionString = $"localhost:{_redis.GetMappedPublicPort(6379)}";

        _factory = new RedisBackedFactory(postgresConnectionString, redisConnectionString);
        _client = _factory.CreateClient();
        _redisConnection = await ConnectionMultiplexer.ConnectAsync(redisConnectionString);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        _factory.Dispose();
        await _redisConnection.DisposeAsync();
        await _redis.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task CustomerExists_WithCacheMiss_ShouldCreateRedisEntryWithTtl()
    {
        var customer = new Customer(
            "Maria Silva",
            "maria.silva@example.com",
            "Rua A, 123",
            new BankingDetails("0001", "123456-7", 150.25m));

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();
            dbContext.Customers.Add(customer);
            await dbContext.SaveChangesAsync();
        }

        var response = await _client.GetAsync($"/api/v1/customers/{customer.Id}/exists");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var exists = await response.Content.ReadFromJsonAsync<bool>();
        exists.Should().BeTrue();

        var database = _redisConnection.GetDatabase();
        var key = CustomerCacheKeys.Exists(customer.Id);
        var cached = await database.StringGetAsync(key);
        var ttl = await database.KeyTimeToLiveAsync(key);

        cached.ToString().Should().Be(bool.TrueString);
        ttl.Should().NotBeNull();
        ttl.Should().BeGreaterThan(TimeSpan.Zero);
        ttl.Should().BeLessThanOrEqualTo(TimeSpan.FromSeconds(30));
    }

    private sealed class RedisBackedFactory(
        string postgresConnectionString,
        string redisConnectionString) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration(configuration =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Postgres"] = postgresConnectionString,
                    ["Redis:ConnectionString"] = redisConnectionString,
                    ["CustomerCache:ExistsTtlSeconds"] = "30",
                    ["Jwt:Issuer"] = "CustomerService",
                    ["Jwt:Audience"] = "BankingSystem",
                    ["Jwt:Secret"] = "change-this-development-secret-at-least-32-chars",
                    ["Jwt:ExpiresInMinutes"] = "60"
                });
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<CustomerDbContext>>();
                services.RemoveAll<ICustomerExistenceCache>();
                services.RemoveAll<IConnectionMultiplexer>();
                services.RemoveAll<CustomerExistenceCacheOptions>();

                services.AddDbContext<CustomerDbContext>(options =>
                {
                    options.UseNpgsql(
                        postgresConnectionString,
                        npgsqlOptions => npgsqlOptions.MigrationsAssembly(typeof(CustomerDbContext).Assembly.FullName));
                });
                services.AddSingleton(new CustomerExistenceCacheOptions { ExistsTtlSeconds = 30 });
                services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));
                services.AddSingleton<ICustomerExistenceCache, RedisCustomerExistenceCache>();
            });
        }
    }
}

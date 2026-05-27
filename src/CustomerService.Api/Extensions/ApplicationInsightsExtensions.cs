namespace CustomerService.Api.Extensions;

public static class ApplicationInsightsExtensions
{
    private const string ConnectionStringKey = "ApplicationInsights:ConnectionString";

    public static IServiceCollection AddOptionalApplicationInsights(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration[ConnectionStringKey];
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return services;
        }

        services.AddApplicationInsightsTelemetry(options =>
        {
            options.ConnectionString = connectionString;
        });

        return services;
    }
}

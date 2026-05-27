using System.Net;
using FluentAssertions;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerService.IntegrationTests;

public sealed class ApplicationInsightsTests
{
    [Fact]
    public async Task Health_WithMissingApplicationInsightsConnectionString_ShouldStartNormally()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        factory.Services.GetService<TelemetryConfiguration>().Should().BeNull();
    }
}

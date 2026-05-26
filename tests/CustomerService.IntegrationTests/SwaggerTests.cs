using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace CustomerService.IntegrationTests;

public sealed class SwaggerTests
{
    [Fact]
    public void SwaggerV1_ShouldIncludeCustomerExistsEndpoint()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        var swaggerProvider = factory.Services.GetRequiredService<ISwaggerProvider>();

        var document = swaggerProvider.GetSwagger("v1");

        document.Paths.Should().ContainKey("/api/v1/customers/{customerId}/exists");
    }
}

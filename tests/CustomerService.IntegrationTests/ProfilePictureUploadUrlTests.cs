using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CustomerService.Domain.Enums;
using FluentAssertions;

namespace CustomerService.IntegrationTests;

public sealed class ProfilePictureUploadUrlTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory = new();
    private HttpClient _client = null!;

    public Task InitializeAsync()
    {
        _client = _factory.CreateClient();
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        _factory.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task CreateProfilePictureUploadUrl_WithCustomerToken_ShouldReturnLocalUploadTarget()
    {
        var customer = await _factory.SeedCustomerAsync();
        await _factory.SeedUserAsync("12345678900", "Customer@123", UserRole.Customer, customer.Id);
        var token = await LoginAndGetTokenAsync("12345678900", "Customer@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync(
            "/api/v1/customers/profile-picture/upload-url",
            new
            {
                fileName = "profile.png",
                contentType = "image/png",
                fileSizeInBytes = 1024
            });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("uploadUrl").GetString().Should().StartWith("https://local.blob-storage.invalid/upload/");
        json.GetProperty("blobUrl").GetString().Should().StartWith("https://local.blob-storage.invalid/");
        json.GetProperty("blobName").GetString().Should().Contain(customer.Id.ToString());
        json.GetProperty("httpMethod").GetString().Should().Be("PUT");
        json.GetProperty("requiredHeaders").GetProperty("x-ms-blob-type").GetString().Should().Be("BlockBlob");
    }

    [Fact]
    public async Task CreateProfilePictureUploadUrl_WithoutToken_ShouldReturnUnauthorized()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/customers/profile-picture/upload-url",
            new
            {
                fileName = "profile.png",
                contentType = "image/png",
                fileSizeInBytes = 1024
            });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateProfilePictureUploadUrl_WithAdminToken_ShouldReturnForbidden()
    {
        await _factory.SeedUserAsync("00000000000", "Admin@123", UserRole.Admin);
        var token = await LoginAndGetTokenAsync("00000000000", "Admin@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync(
            "/api/v1/customers/profile-picture/upload-url",
            new
            {
                fileName = "profile.png",
                contentType = "image/png",
                fileSizeInBytes = 1024
            });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateProfilePictureUploadUrl_WithCustomerTokenWithoutCustomerClaim_ShouldReturnForbidden()
    {
        await _factory.SeedUserAsync("12345678900", "Customer@123", UserRole.Customer);
        var token = await LoginAndGetTokenAsync("12345678900", "Customer@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync(
            "/api/v1/customers/profile-picture/upload-url",
            new
            {
                fileName = "profile.png",
                contentType = "image/png",
                fileSizeInBytes = 1024
            });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateProfilePictureUploadUrl_WithMissingCustomer_ShouldReturnNotFound()
    {
        await _factory.SeedUserAsync("12345678900", "Customer@123", UserRole.Customer, Guid.NewGuid());
        var token = await LoginAndGetTokenAsync("12345678900", "Customer@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync(
            "/api/v1/customers/profile-picture/upload-url",
            new
            {
                fileName = "profile.png",
                contentType = "image/png",
                fileSizeInBytes = 1024
            });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateProfilePictureUploadUrl_WithInvalidMetadata_ShouldReturnBadRequest()
    {
        var customer = await _factory.SeedCustomerAsync();
        await _factory.SeedUserAsync("12345678900", "Customer@123", UserRole.Customer, customer.Id);
        var token = await LoginAndGetTokenAsync("12345678900", "Customer@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync(
            "/api/v1/customers/profile-picture/upload-url",
            new
            {
                fileName = "profile.gif",
                contentType = "image/gif",
                fileSizeInBytes = 1024
            });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task<string> LoginAndGetTokenAsync(string cpf, string password)
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new { cpf, password });
        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<LoginTokenResponse>();
        return tokenResponse!.AccessToken;
    }

    private sealed record LoginTokenResponse(string AccessToken, string TokenType, int ExpiresIn);
}

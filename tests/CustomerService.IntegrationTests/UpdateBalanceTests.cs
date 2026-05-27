using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CustomerService.Domain.Enums;
using FluentAssertions;

namespace CustomerService.IntegrationTests;

public sealed class UpdateBalanceTests : IAsyncLifetime
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
    public async Task UpdateBalance_WithCustomerToken_ShouldTransferBalance()
    {
        var sender = await _factory.SeedCustomerAsync(name: "Sender", balance: 150m);
        var receiver = await _factory.SeedCustomerAsync(name: "Receiver", balance: 25m);
        await AuthenticateAsync("12345678900", "Customer@123", UserRole.Customer, sender.Id);

        var response = await _client.PostAsJsonAsync("/api/v1/customers/update-balance", new
        {
            receiverId = receiver.Id,
            amount = 40m
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("senderId").GetGuid().Should().Be(sender.Id);
        json.GetProperty("receiverId").GetGuid().Should().Be(receiver.Id);
        json.GetProperty("amount").GetDecimal().Should().Be(40m);
        json.GetProperty("senderBalance").GetDecimal().Should().Be(110m);

        var updatedSender = await _factory.GetCustomerAsync(sender.Id);
        var updatedReceiver = await _factory.GetCustomerAsync(receiver.Id);
        updatedSender!.BankingDetails.Balance.Should().Be(110m);
        updatedReceiver!.BankingDetails.Balance.Should().Be(65m);
    }

    [Fact]
    public async Task UpdateBalance_WithoutToken_ShouldReturnUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/customers/update-balance", new
        {
            receiverId = Guid.NewGuid(),
            amount = 10m
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateBalance_WithAdminToken_ShouldReturnForbidden()
    {
        await AuthenticateAsync("00000000000", "Admin@123", UserRole.Admin);

        var response = await _client.PostAsJsonAsync("/api/v1/customers/update-balance", new
        {
            receiverId = Guid.NewGuid(),
            amount = 10m
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateBalance_WithCustomerTokenWithoutCustomerClaim_ShouldReturnForbidden()
    {
        await AuthenticateAsync("12345678900", "Customer@123", UserRole.Customer);

        var response = await _client.PostAsJsonAsync("/api/v1/customers/update-balance", new
        {
            receiverId = Guid.NewGuid(),
            amount = 10m
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateBalance_WithInvalidRequest_ShouldReturnValidationError()
    {
        var sender = await _factory.SeedCustomerAsync(balance: 150m);
        await AuthenticateAsync("12345678900", "Customer@123", UserRole.Customer, sender.Id);

        var response = await _client.PostAsJsonAsync("/api/v1/customers/update-balance", new
        {
            receiverId = Guid.Empty,
            amount = 0m
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Validation failed");
    }

    [Fact]
    public async Task UpdateBalance_WithMissingReceiver_ShouldReturnNotFoundAndKeepSenderBalance()
    {
        var sender = await _factory.SeedCustomerAsync(balance: 150m);
        await AuthenticateAsync("12345678900", "Customer@123", UserRole.Customer, sender.Id);

        var response = await _client.PostAsJsonAsync("/api/v1/customers/update-balance", new
        {
            receiverId = Guid.NewGuid(),
            amount = 40m
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var updatedSender = await _factory.GetCustomerAsync(sender.Id);
        updatedSender!.BankingDetails.Balance.Should().Be(150m);
    }

    [Fact]
    public async Task UpdateBalance_WithInsufficientBalance_ShouldReturnConflictAndKeepBalances()
    {
        var sender = await _factory.SeedCustomerAsync(name: "Sender", balance: 10m);
        var receiver = await _factory.SeedCustomerAsync(name: "Receiver", balance: 25m);
        await AuthenticateAsync("12345678900", "Customer@123", UserRole.Customer, sender.Id);

        var response = await _client.PostAsJsonAsync("/api/v1/customers/update-balance", new
        {
            receiverId = receiver.Id,
            amount = 40m
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var updatedSender = await _factory.GetCustomerAsync(sender.Id);
        var updatedReceiver = await _factory.GetCustomerAsync(receiver.Id);
        updatedSender!.BankingDetails.Balance.Should().Be(10m);
        updatedReceiver!.BankingDetails.Balance.Should().Be(25m);
    }

    [Fact]
    public async Task UpdateBalance_WithSelfTransfer_ShouldReturnBadRequestAndKeepBalance()
    {
        var sender = await _factory.SeedCustomerAsync(balance: 150m);
        await AuthenticateAsync("12345678900", "Customer@123", UserRole.Customer, sender.Id);

        var response = await _client.PostAsJsonAsync("/api/v1/customers/update-balance", new
        {
            receiverId = sender.Id,
            amount = 40m
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var updatedSender = await _factory.GetCustomerAsync(sender.Id);
        updatedSender!.BankingDetails.Balance.Should().Be(150m);
    }

    private async Task AuthenticateAsync(string cpf, string password, UserRole role, Guid? customerId = null)
    {
        await _factory.SeedUserAsync(cpf, password, role, customerId);
        var token = await LoginAndGetTokenAsync(cpf, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
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

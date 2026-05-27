using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using CustomerService.Domain.Enums;
using FluentAssertions;

namespace CustomerService.IntegrationTests;

public sealed class AuthRbacTests : IAsyncLifetime
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
    public async Task Login_WithValidCredentials_ShouldReturnJwtWithRequiredClaims()
    {
        var customerId = Guid.NewGuid();
        await _factory.SeedUserAsync("12345678900", "Customer@123", UserRole.Customer, customerId);

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            cpf = "123.456.789-00",
            password = "Customer@123"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var tokenResponse = await response.Content.ReadFromJsonAsync<LoginTokenResponse>();
        tokenResponse.Should().NotBeNull();
        tokenResponse!.TokenType.Should().Be("Bearer");
        tokenResponse.ExpiresIn.Should().Be(3600);

        var token = new JwtSecurityTokenHandler().ReadJwtToken(tokenResponse.AccessToken);
        token.Claims.Should().Contain(claim => claim.Type == "sub");
        token.Claims.Should().Contain(claim => claim.Type == "cpf" && claim.Value == "12345678900");
        token.Claims.Should().Contain(claim => claim.Type == "role" && claim.Value == "Customer");
        token.Claims.Should().Contain(claim => claim.Type == "customer_id" && claim.Value == customerId.ToString());
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnGenericUnauthorizedError()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            cpf = "99999999999",
            password = "Wrong@123"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Invalid CPF or password.");
    }

    [Fact]
    public async Task CreateUser_WithoutToken_ShouldReturnUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/customers", new
        {
            cpf = "12345678900",
            password = "Customer@123",
            role = "Customer",
            name = "Maria Silva",
            email = "maria.silva@example.com",
            address = "Rua A, 123",
            agency = "0001",
            checkingAccountNumber = "123456-7",
            balance = 250.75m
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateUser_WithCustomerToken_ShouldReturnForbidden()
    {
        await _factory.SeedUserAsync("12345678900", "Customer@123", UserRole.Customer);
        var token = await LoginAndGetTokenAsync("12345678900", "Customer@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync("/api/v1/customers", new
        {
            cpf = "22222222222",
            password = "Customer@123",
            role = "Customer",
            name = "Maria Silva",
            email = "maria.silva@example.com",
            address = "Rua A, 123",
            agency = "0001",
            checkingAccountNumber = "123456-7",
            balance = 250.75m
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateUser_WithAdminToken_ShouldCreateUserAndRejectDuplicateCpf()
    {
        await _factory.SeedUserAsync("00000000000", "Admin@123", UserRole.Admin);
        var token = await LoginAndGetTokenAsync("00000000000", "Admin@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResponse = await _client.PostAsJsonAsync("/api/v1/customers", new
        {
            cpf = "123.456.789-00",
            password = "Customer@123",
            role = "Customer",
            name = "Maria Silva",
            email = "maria.silva@example.com",
            address = "Rua A, 123",
            agency = "0001",
            checkingAccountNumber = "123456-7",
            balance = 250.75m
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var json = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("cpf").GetString().Should().Be("12345678900");
        json.GetProperty("customer").GetProperty("id").GetGuid().Should().NotBeEmpty();
        json.GetProperty("customer").GetProperty("name").GetString().Should().Be("Maria Silva");
        json.GetProperty("customer").GetProperty("email").GetString().Should().Be("maria.silva@example.com");
        json.GetProperty("customer").GetProperty("address").GetString().Should().Be("Rua A, 123");
        json.GetProperty("customer").GetProperty("agency").GetString().Should().Be("0001");
        json.GetProperty("customer").GetProperty("checkingAccountNumber").GetString().Should().Be("123456-7");
        json.GetProperty("customer").GetProperty("balance").GetDecimal().Should().Be(250.75m);
        json.TryGetProperty("passwordHash", out _).Should().BeFalse();

        var duplicateResponse = await _client.PostAsJsonAsync("/api/v1/customers", new
        {
            cpf = "12345678900",
            password = "Customer@123",
            role = "Customer",
            name = "Maria Silva",
            email = "maria.silva@example.com",
            address = "Rua A, 123",
            agency = "0001",
            checkingAccountNumber = "123456-7"
        });

        duplicateResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetCustomer_WithAdminTokenAndRouteId_ShouldReturnRequestedCustomer()
    {
        var customer = await _factory.SeedCustomerAsync(name: "Cliente Admin View");
        await _factory.SeedUserAsync("00000000000", "Admin@123", UserRole.Admin);
        var token = await LoginAndGetTokenAsync("00000000000", "Admin@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync($"/api/v1/customers/{customer.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("id").GetGuid().Should().Be(customer.Id);
        json.GetProperty("name").GetString().Should().Be("Cliente Admin View");
    }

    [Fact]
    public async Task GetCustomer_WithCustomerTokenAndNoRouteId_ShouldReturnAuthenticatedCustomer()
    {
        var customer = await _factory.SeedCustomerAsync(name: "Cliente Logado");
        await _factory.SeedUserAsync("12345678900", "Customer@123", UserRole.Customer, customer.Id);
        var token = await LoginAndGetTokenAsync("12345678900", "Customer@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/v1/customers");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("id").GetGuid().Should().Be(customer.Id);
        json.GetProperty("name").GetString().Should().Be("Cliente Logado");
    }

    [Fact]
    public async Task GetCustomer_WithCustomerTokenAndRouteId_ShouldReturnForbidden()
    {
        var customer = await _factory.SeedCustomerAsync();
        await _factory.SeedUserAsync("12345678900", "Customer@123", UserRole.Customer, customer.Id);
        var token = await LoginAndGetTokenAsync("12345678900", "Customer@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync($"/api/v1/customers/{customer.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CustomerExists_WithAdminTokenAndExistingCustomer_ShouldReturnTrue()
    {
        var customer = await _factory.SeedCustomerAsync();
        await _factory.SeedUserAsync("00000000000", "Admin@123", UserRole.Admin);
        var token = await LoginAndGetTokenAsync("00000000000", "Admin@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync($"/api/v1/customers/{customer.Id}/exists");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var exists = await response.Content.ReadFromJsonAsync<bool>();
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task CustomerExists_WithAdminTokenAndMissingCustomer_ShouldReturnFalse()
    {
        await _factory.SeedUserAsync("00000000000", "Admin@123", UserRole.Admin);
        var token = await LoginAndGetTokenAsync("00000000000", "Admin@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync($"/api/v1/customers/{Guid.NewGuid()}/exists");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var exists = await response.Content.ReadFromJsonAsync<bool>();
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task CustomerExists_WithInvalidCustomerId_ShouldReturnClientError()
    {
        await _factory.SeedUserAsync("00000000000", "Admin@123", UserRole.Admin);
        var token = await LoginAndGetTokenAsync("00000000000", "Admin@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/v1/customers/not-a-guid/exists");

        ((int)response.StatusCode).Should().BeInRange(400, 499);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotBe("false");
    }

    [Fact]
    public async Task UpdateCustomer_WithCustomerTokenAndNoRouteId_ShouldUpdateAuthenticatedCustomer()
    {
        var customer = await _factory.SeedCustomerAsync(name: "Nome Antigo");
        await _factory.SeedUserAsync("12345678900", "Customer@123", UserRole.Customer, customer.Id);
        var token = await LoginAndGetTokenAsync("12345678900", "Customer@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PatchAsJsonAsync("/api/v1/customers", new
        {
            name = "Nome Atualizado",
            profileImageUrl = "https://storage.example.com/customers/profile.png"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("customerId").GetGuid().Should().Be(customer.Id);

        var detailsResponse = await _client.GetAsync("/api/v1/customers");
        var details = await detailsResponse.Content.ReadFromJsonAsync<JsonElement>();
        details.GetProperty("name").GetString().Should().Be("Nome Atualizado");
        details.GetProperty("profilePictureUrl").GetString().Should().Be("https://storage.example.com/customers/profile.png");
    }

    [Fact]
    public async Task UpdateCustomer_WithCustomerTokenAndRouteId_ShouldReturnForbidden()
    {
        var customer = await _factory.SeedCustomerAsync();
        await _factory.SeedUserAsync("12345678900", "Customer@123", UserRole.Customer, customer.Id);
        var token = await LoginAndGetTokenAsync("12345678900", "Customer@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PatchAsJsonAsync($"/api/v1/customers/{customer.Id}", new
        {
            name = "Tentativa Bloqueada"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateCustomer_WithAdminTokenAndRouteId_ShouldUpdateRequestedCustomer()
    {
        var customer = await _factory.SeedCustomerAsync(name: "Nome Antigo");
        await _factory.SeedUserAsync("00000000000", "Admin@123", UserRole.Admin);
        var token = await LoginAndGetTokenAsync("00000000000", "Admin@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PatchAsJsonAsync($"/api/v1/customers/{customer.Id}", new
        {
            name = "Nome Admin"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("customerId").GetGuid().Should().Be(customer.Id);
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

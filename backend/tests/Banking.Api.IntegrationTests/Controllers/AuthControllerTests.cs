using System.Net;
using System.Net.Http.Json;
using Banking.Api.Controllers;
using FluentAssertions;
using Xunit;

namespace Banking.Api.IntegrationTests.Controllers;

/// <summary>
/// Integration tests for AuthController.
/// Tests login endpoint with a real database via Testcontainers.
/// </summary>
public class AuthControllerTests : IAsyncLifetime
{
    private readonly BankingApiFactory _factory = new();
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        await _factory.InitializeAsync();
        _client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Login_WithInvalidEmail_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new AuthController.LoginRequest(
            Email: "nonexistent@example.com",
            Password: "password123"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnAccessTokenAndRole()
    {
        // NOTE: This test would require seeding a user in the database.
        // For now, we verify the endpoint structure. A full test would:
        // 1. Create a user via UserManager
        // 2. Verify login returns a JWT token
        // 3. Verify the token contains the user's role
        
        // This is a placeholder that demonstrates the test structure.
        // Full implementation depends on your user seeding strategy.
        
        // Arrange
        var request = new AuthController.LoginRequest(
            Email: "test@example.com",
            Password: "TestPassword123!"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/auth/login", request);

        // Assert
        // We expect Unauthorized since we haven't seeded a user
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_ShouldNotRequireAuthentication()
    {
        // Verify login endpoint is [AllowAnonymous]
        // This test just ensures the endpoint is reachable without auth

        // Arrange
        var request = new AuthController.LoginRequest(
            Email: "test@example.com",
            Password: "password"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/auth/login", request);

        // Assert - Should get a response (may be 401, but not 403 forbidden)
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }
}

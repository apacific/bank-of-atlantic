using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Banking.Application.Customers.Commands;
using Banking.Application.Customers.DTOs;
using FluentAssertions;
using Xunit;
using Microsoft.IdentityModel.Tokens;

namespace Banking.Api.IntegrationTests.Controllers;

/// <summary>
/// Integration tests for CustomersController.
/// Tests customer CRUD operations with a real database via Testcontainers.
/// </summary>
public class CustomersControllerTests : IAsyncLifetime
{
    private readonly BankingApiFactory _factory = new();
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        await _factory.InitializeAsync();
        _client = _factory.CreateClient();
        
        // Add JWT token to all requests for authentication
        var token = GenerateTestJwt();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    private string GenerateTestJwt()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("test_secret_key_that_is_long_enough_for_hs256_algorithm_padding"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim(ClaimTypes.Role, "Employee")
        };

        var token = new JwtSecurityToken(
            issuer: "Banking.Api",
            audience: "Banking.Frontend",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task DisposeAsync()
    {
        await _factory.ResetDatabaseAsync();
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    private static CreateCustomerCommand CreateValidCommand()
    {
        return new CreateCustomerCommand(
            FirstName: "John",
            LastName: "Doe",
            Suffix: null,
            Title: "Mr.",
            SsnTin: "123-45-6789",
            Email: "john.doe@example.com",
            Street: "123 Main St",
            City: "Springfield",
            State: "IL",
            PostalCode: "62701",
            Country: "USA"
        );
    }

    private static async Task<T> ReadJsonAsync<T>(HttpContent content)
    {
        var stream = await content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync<T>(stream) ?? throw new InvalidOperationException("Deserialization failed");
    }

    [Fact]
    public async Task CreateCustomer_WithValidData_ShouldReturnCreatedAtAction()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        var response = await _client.PostAsJsonAsync("/customers", command);

        // Assert - Just verify the status code for now
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        // Verify response contains a Location header or ID in body
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateCustomer_WithDuplicateEmail_ShouldReturnConflict()
    {
        // Arrange
        var command = CreateValidCommand();
        
        // Create first customer
        var response1 = await _client.PostAsJsonAsync("/customers", command);
        response1.StatusCode.Should().Be(HttpStatusCode.Created);

        // Try to create another with same email
        var duplicateCommand = command with { SsnTin = "987-65-4321" };

        // Act
        var response2 = await _client.PostAsJsonAsync("/customers", duplicateCommand);

        // Assert - API returns Conflict for duplicate email
        response2.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateCustomer_WithDuplicateSSN_ShouldReturnConflict()
    {
        // Arrange
        var command = CreateValidCommand();
        
        // Create first customer
        var response1 = await _client.PostAsJsonAsync("/customers", command);
        response1.StatusCode.Should().Be(HttpStatusCode.Created);

        // Try to create another with same SSN
        var duplicateCommand = command with { Email = "different@example.com" };

        // Act
        var response2 = await _client.PostAsJsonAsync("/customers", duplicateCommand);

        // Assert - API returns Conflict for duplicate SSN
        response2.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetAllCustomers_ShouldReturnOk()
    {
        // Arrange
        var command1 = CreateValidCommand();
        var command2 = CreateValidCommand() with 
        { 
            Email = "jane.doe@example.com",
            FirstName = "Jane",
            SsnTin = "987-65-4321"
        };

        // Create two customers
        await _client.PostAsJsonAsync("/customers", command1);
        await _client.PostAsJsonAsync("/customers", command2);

        // Act
        var response = await _client.GetAsync("/customers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetCustomerById_WithExistingId_ShouldReturnOk()
    {
        // Arrange
        var command = CreateValidCommand();
        var createResponse = await _client.PostAsJsonAsync("/customers", command);
        
        // Extract ID from Location header
        var location = createResponse.Headers.Location;
        location.Should().NotBeNull();
        var customerId = location!.Segments.Last();

        // Act
        var response = await _client.GetAsync($"/customers/{customerId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetCustomerById_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/customers/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateCustomer_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var createCommand = CreateValidCommand();
        var createResponse = await _client.PostAsJsonAsync("/customers", createCommand);
        
        // Extract ID from Location header
        var location = createResponse.Headers.Location;
        location.Should().NotBeNull();
        var customerId = location!.Segments.Last();

        var updateCommand = new UpdateCustomerCommand(
            Id: Guid.Parse(customerId),
            FirstName: "Updated",
            LastName: "Name",
            Suffix: "Jr.",
            Title: "Dr.",
            SsnTin: "123-45-6789",
            Email: "updated@example.com",
            Street: "456 Oak Ave",
            City: "Chicago",
            State: "IL",
            PostalCode: "60601",
            Country: "USA"
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/customers/{customerId}", updateCommand);

        // Assert - PUT may return 200 OK or 204 No Content
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteCustomer_WithExistingId_ShouldReturnNoContent()
    {
        // Arrange
        var command = CreateValidCommand();
        var createResponse = await _client.PostAsJsonAsync("/customers", command);
        
        // Extract ID from Location header
        var location = createResponse.Headers.Location;
        location.Should().NotBeNull();
        var customerId = location!.Segments.Last();

        // Act
        var response = await _client.DeleteAsync($"/customers/{customerId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify customer is deleted
        var getResponse = await _client.GetAsync($"/customers/{customerId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

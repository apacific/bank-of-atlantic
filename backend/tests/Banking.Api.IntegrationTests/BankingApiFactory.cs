using Banking.Infrastructure;
using Banking.Infrastructure.Identity;
using Banking.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using Testcontainers.PostgreSql;
using Xunit;

namespace Banking.Api.IntegrationTests;

/// <summary>
/// Fixture that manages a test database container and WebApplicationFactory for integration tests.
/// Uses Testcontainers to spin up an ephemeral PostgreSQL instance per test session.
/// </summary>
public sealed class BankingApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private PostgreSqlContainer? _container;

    public async Task InitializeAsync()
    {
        // Create and start PostgreSQL container
        _container = new PostgreSqlBuilder()
            .WithDatabase("banking_test")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .WithCleanUp(true)
            .Build();

        await _container.StartAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        if (_container != null)
        {
            await _container.StopAsync();
            await _container.DisposeAsync();
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Override JWT configuration for testing
            var testSettings = new Dictionary<string, string>
            {
                {"Jwt:Key", "test_secret_key_that_is_long_enough_for_hs256_algorithm_padding"},
                {"Jwt:Issuer", "Banking.Api"},
                {"Jwt:Audience", "Banking.Frontend"}
            };
            config.AddInMemoryCollection(testSettings!);
        });

        builder.ConfigureServices((context, services) =>
        {
            // Add test authentication scheme that bypasses JWT validation
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", null);

            // Remove existing DbContext registrations
            var bdContextDescriptor = services.FirstOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<BankingDbContext>));
            if (bdContextDescriptor != null)
                services.Remove(bdContextDescriptor);

            var idContextDescriptor = services.FirstOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<BankingIdentityDbContext>));
            if (idContextDescriptor != null)
                services.Remove(idContextDescriptor);

            // Add test DbContext with Testcontainer connection string
            var connectionString = _container!.GetConnectionString();
            
            services.AddDbContext<BankingDbContext>(opt =>
                opt.UseNpgsql(connectionString));

            services.AddDbContext<BankingIdentityDbContext>(opt =>
                opt.UseNpgsql(connectionString));

            // Ensure database schema is created and migrations applied
            using (var scope = services.BuildServiceProvider().CreateScope())
            {
                var bankingDb = scope.ServiceProvider.GetRequiredService<BankingDbContext>();
                var identityDb = scope.ServiceProvider.GetRequiredService<BankingIdentityDbContext>();

                // Apply migrations
                bankingDb.Database.Migrate();
                identityDb.Database.Migrate();

                // Seed roles if they don't exist
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
                SeedRoles(roleManager).GetAwaiter().GetResult();
            }
        });

        base.ConfigureWebHost(builder);
    }

    private static async Task SeedRoles(RoleManager<IdentityRole<Guid>> roleManager)
    {
        var roles = new[] { "Employee", "Manager" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<Guid> { Name = role });
        }
    }

    /// <summary>
    /// Clears all data from test database (useful between test suites).
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BankingDbContext>();
        
        // Delete all data
        context.Accounts.RemoveRange(context.Accounts);
        context.Customers.RemoveRange(context.Customers);
        await context.SaveChangesAsync();
    }
}

/// <summary>
/// Test authentication handler that automatically authenticates all requests for testing purposes.
/// </summary>
public sealed class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) 
        : base(options, logger, encoder) { }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // For testing, just create a claims principal with test claims
        var claims = new[]
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, "test@example.com"),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Employee")
        };

        var identity = new System.Security.Claims.ClaimsIdentity(claims, "Test");
        var principal = new System.Security.Claims.ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return await Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

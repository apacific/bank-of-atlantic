using Banking.Api.Middleware;
using Banking.Application;
using Banking.Infrastructure;

using Banking.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using System.Text;
using System.Text.Json.Serialization;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Banking.Api.IntegrationTests")]

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddCors(o => o.AddPolicy("frontend", p =>
    p.WithOrigins("http://localhost:5173")
     .AllowAnyHeader()
     .AllowAnyMethod()));

static string GetConn(IConfiguration cfg) =>
    cfg.GetConnectionString("Default")
    ?? cfg.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Missing connection string. Add ConnectionStrings:Default (or DefaultConnection).");

builder.Services.AddDbContext<BankingIdentityDbContext>(opt =>
    opt.UseNpgsql(GetConn(builder.Configuration)));

builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<BankingIdentityDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

var jwtSection = builder.Configuration.GetSection("Jwt");
var issuer = jwtSection["Issuer"] ?? "Banking.Api";
var audience = jwtSection["Audience"] ?? "Banking.Frontend";
var key = jwtSection["Key"] ?? throw new InvalidOperationException("Missing configuration: Jwt:Key");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,

            ValidateAudience = true,
            ValidAudience = audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2),
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors("frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    var identityDb = scope.ServiceProvider.GetRequiredService<BankingIdentityDbContext>();
    await identityDb.Database.MigrateAsync();

    await SeedIdentityAsync(scope.ServiceProvider, app.Configuration);
}

app.Run();

static async Task SeedIdentityAsync(IServiceProvider sp, IConfiguration cfg)
{
    const string EmployeeRole = "Employee";
    const string ManagerRole = "Manager";

    var roleManager = sp.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();

    if (!await roleManager.RoleExistsAsync(EmployeeRole))
        await roleManager.CreateAsync(new IdentityRole<Guid>(EmployeeRole));

    if (!await roleManager.RoleExistsAsync(ManagerRole))
        await roleManager.CreateAsync(new IdentityRole<Guid>(ManagerRole));

    async Task EnsureUserAsync(string email, string password, string role)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = email,
                UserName = email,
                EmailConfirmed = true
            };

            var create = await userManager.CreateAsync(user, password);
            if (!create.Succeeded)
                throw new InvalidOperationException(string.Join("; ", create.Errors.Select(e => e.Description)));
        }
        else
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var reset = await userManager.ResetPasswordAsync(user, token, password);
            if (!reset.Succeeded)
                throw new InvalidOperationException(string.Join("; ", reset.Errors.Select(e => e.Description)));
        }

        if (!await userManager.IsInRoleAsync(user, role))
            await userManager.AddToRoleAsync(user, role);
    }

    var seed = cfg.GetSection("SeedUsers");
    await EnsureUserAsync(seed["EmployeeEmail"] ?? "employee@atlantic.local", seed["EmployeePassword"] ?? "Passw0rd!", EmployeeRole);
    await EnsureUserAsync(seed["ManagerEmail"] ?? "manager@atlantic.local", seed["ManagerPassword"] ?? "Passw0rd!123", ManagerRole);
}

public partial class Program { }

using Banking.Application.Abstractions;
using Banking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Banking.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration cfg)
    {
        var cs =
            cfg.GetConnectionString("Default")
            ?? cfg.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Missing connection string. Add ConnectionStrings:Default (or DefaultConnection)."
            );

        services.AddDbContext<BankingDbContext>(opt => opt.UseNpgsql(cs));
        services.AddScoped<IBankingDbContext>(sp => sp.GetRequiredService<BankingDbContext>());

        return services;
    }
}

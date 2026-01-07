using Banking.Domain.Accounts;
using Banking.Domain.Customers;
using Microsoft.EntityFrameworkCore;

namespace Banking.Application.Abstractions;

public interface IBankingDbContext
{
    DbSet<Customer> Customers { get; }
    DbSet<Account> Accounts { get; }
    Task<int> SaveChangesAsync(CancellationToken ct);
}

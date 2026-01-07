using Banking.Application.Abstractions;
using Banking.Domain.Accounts;
using Banking.Domain.Customers;
using Microsoft.EntityFrameworkCore;

namespace Banking.Infrastructure.Persistence;

public sealed class BankingDbContext(DbContextOptions<BankingDbContext> options)
    : DbContext(options), IBankingDbContext
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Account> Accounts => Set<Account>();

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    => base.SaveChangesAsync(ct);


    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Customer>(e =>
        {
            e.HasKey(x => x.Id);

            e.OwnsOne(x => x.MailingAddress, owned =>
            {
                owned.Property(x => x.Street).HasMaxLength(200).IsRequired();
                owned.Property(x => x.City).HasMaxLength(120).IsRequired();
                owned.Property(x => x.State).HasMaxLength(120).IsRequired();
                owned.Property(x => x.PostalCode).HasMaxLength(30).IsRequired();
                owned.Property(x => x.Country).HasMaxLength(120).IsRequired();
            });

            e.HasIndex(x => x.EmailNormalized).IsUnique();
            e.HasIndex(x => x.SsnTinNormalized).IsUnique();

            e.Property(x => x.CustomerSince).HasColumnType("date");
        });

        b.Entity<Account>(e =>
        {
            e.HasKey(x => x.Id);

            e.Property(x => x.DateOpened).HasColumnType("date");
            e.Property(x => x.AvailableBalance).HasColumnType("numeric(18,2)");

            e.HasIndex(x => x.AccountNumber).IsUnique();

            e.HasOne<Customer>()
              .WithMany(c => c.Accounts)
              .HasForeignKey(a => a.CustomerId)
              .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

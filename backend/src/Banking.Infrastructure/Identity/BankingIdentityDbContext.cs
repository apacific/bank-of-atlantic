using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Banking.Infrastructure.Identity;

public sealed class BankingIdentityDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public BankingIdentityDbContext(DbContextOptions<BankingIdentityDbContext> options)
        : base(options) { }
}

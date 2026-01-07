using Banking.Application.Abstractions;
using Banking.Application.Accounts.DTOs;
using Banking.Application.Common.Errors;
using Banking.Domain.Accounts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Banking.Application.Accounts.Commands;

public sealed record CreateAccountCommand(
    Guid CustomerId,
    AccountType? AccountType
) : IRequest<AccountDetailDto>;

public sealed class CreateAccountHandler(IBankingDbContext db)
    : IRequestHandler<CreateAccountCommand, AccountDetailDto>
{
    public async Task<AccountDetailDto> Handle(CreateAccountCommand r, CancellationToken ct)
    {
        var customerExists = await db.Customers.AnyAsync(c => c.Id == r.CustomerId, ct);
        if (!customerExists) throw new NotFoundAppException("Customer not found.");

        var accountType = r.AccountType!.Value;

        var dateOpened = DateOnly.FromDateTime(DateTime.UtcNow);

        static string GenerateAccountNumber() =>
            Random.Shared.NextInt64(10_0000_0000L, 99_9999_9999L).ToString();

        var accountNumber = GenerateAccountNumber();
        while (await db.Accounts.AnyAsync(a => a.AccountNumber == accountNumber, ct))
            accountNumber = GenerateAccountNumber();

        var account = new Account(r.CustomerId, accountNumber, accountType, dateOpened);

        db.Accounts.Add(account);
        await db.SaveChangesAsync(ct);

        return new AccountDetailDto(
            account.Id,
            account.CustomerId,
            account.AccountNumber,
            account.AccountType.ToString(),
            account.DateOpened.ToString("yyyy-MM-dd"),
            account.AvailableBalance
        );
    }
}

using Banking.Application.Abstractions;
using Banking.Application.Accounts.DTOs;
using Banking.Application.Common.Errors;
using Banking.Domain.Accounts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Banking.Application.Accounts.Commands;

public sealed record UpdateAccountCommand(
    Guid CustomerId,
    Guid AccountId,
    AccountType AccountType,
    decimal AvailableBalance
) : IRequest<AccountDetailDto>;

public sealed class UpdateAccountHandler(IBankingDbContext db)
    : IRequestHandler<UpdateAccountCommand, AccountDetailDto>
{
    public async Task<AccountDetailDto> Handle(UpdateAccountCommand r, CancellationToken ct)
    {
        var a = await db.Accounts
            .FirstOrDefaultAsync(x => x.Id == r.AccountId && x.CustomerId == r.CustomerId, ct);

        if (a is null) throw new NotFoundAppException("Account not found.");

        a.UpdateEditable(r.AccountType, r.AvailableBalance);
        await db.SaveChangesAsync(ct);

        return new AccountDetailDto(
            a.Id,
            a.CustomerId,
            a.AccountNumber,
            a.AccountType.ToString(),
            a.DateOpened.ToString("yyyy-MM-dd"),
            a.AvailableBalance
        );
    }
}

using Banking.Application.Abstractions;
using Banking.Application.Accounts.DTOs;
using Banking.Application.Common.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Banking.Application.Accounts.Queries;

public sealed record GetAccountByIdQuery(Guid CustomerId, Guid AccountId) : IRequest<AccountDetailDto>;

public sealed class GetAccountByIdHandler(IBankingDbContext db)
    : IRequestHandler<GetAccountByIdQuery, AccountDetailDto>
{
    public async Task<AccountDetailDto> Handle(GetAccountByIdQuery r, CancellationToken ct)
    {
        var a = await db.Accounts.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == r.AccountId && x.CustomerId == r.CustomerId, ct);

        if (a is null) throw new NotFoundAppException("Account not found.");

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

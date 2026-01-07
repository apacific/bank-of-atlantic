using Banking.Application.Abstractions;
using Banking.Application.Common.Errors;
using Banking.Domain.Accounts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Banking.Application.Accounts.Commands;

public sealed record DeleteAccountCommand(Guid CustomerId, Guid AccountId) : IRequest<Unit>;

public sealed class DeleteAccountHandler(IBankingDbContext db)
    : IRequestHandler<DeleteAccountCommand, Unit>
{
    public async Task<Unit> Handle(DeleteAccountCommand r, CancellationToken ct)
    {
        var account = await db.Accounts
            .FirstOrDefaultAsync(a => a.Id == r.AccountId && a.CustomerId == r.CustomerId, ct);

        if (account is null) throw new NotFoundAppException("Account not found.");

        var isCreditCard = account.AccountType == AccountType.CreditCard;
        var balanceIsZero = account.AvailableBalance == 0m;

        if (!isCreditCard && !balanceIsZero)
        {
            throw new ConflictAppException(
                "Account cannot be deleted unless the available balance is 0 (except credit cards).",
                new Dictionary<string, string[]>
                {
                    ["AvailableBalance"] = new[] { "Must be 0 to delete non-credit-card accounts." }
                }
            );
        }

        db.Accounts.Remove(account);
        await db.SaveChangesAsync(ct);

        return Unit.Value;
    }
}

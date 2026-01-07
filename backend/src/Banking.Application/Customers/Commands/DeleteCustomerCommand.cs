using Banking.Application.Abstractions;
using Banking.Application.Common.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Banking.Application.Customers.Commands;

public sealed record DeleteCustomerCommand(Guid Id) : IRequest<Unit>;

public sealed class DeleteCustomerHandler(IBankingDbContext db)
    : IRequestHandler<DeleteCustomerCommand, Unit>
{
    public async Task<Unit> Handle(DeleteCustomerCommand r, CancellationToken ct)
    {
        var customer = await db.Customers
            .Include(c => c.Accounts)
            .FirstOrDefaultAsync(c => c.Id == r.Id, ct);

        if (customer is null) throw new NotFoundAppException("Customer not found.");

        if (customer.Accounts.Count > 0)
            throw new ConflictAppException("Customer has associated accounts and cannot be deleted.");

        db.Customers.Remove(customer);
        await db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

using Banking.Application.Abstractions;
using Banking.Application.Common.Errors;
using Banking.Application.Customers.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Banking.Application.Customers.Queries;

public sealed record GetCustomerByIdQuery(Guid Id) : IRequest<CustomerDetailDto>;

public sealed class GetCustomerByIdHandler(IBankingDbContext db)
    : IRequestHandler<GetCustomerByIdQuery, CustomerDetailDto>
{
    public async Task<CustomerDetailDto> Handle(GetCustomerByIdQuery r, CancellationToken ct)
    {
        var c = await db.Customers
            .AsNoTracking()
            .Include(x => x.Accounts)
            .FirstOrDefaultAsync(x => x.Id == r.Id, ct);

        if (c is null) throw new NotFoundAppException("Customer not found.");

        var accounts = c.Accounts
            .OrderBy(a => a.AccountNumber)
            .Select(a => new AccountSummaryDto(
                a.Id,
                a.AccountNumber,
                a.AccountType.ToString(),
                a.AvailableBalance
            ))
            .ToList();

        return new CustomerDetailDto(
            c.Id,
            c.FirstName,
            c.LastName,
            c.Suffix,
            c.Title,
            c.SsnTin,
            c.Email,
            c.MailingAddress.Street,
            c.MailingAddress.City,
            c.MailingAddress.State,
            c.MailingAddress.PostalCode,
            c.MailingAddress.Country,
            c.CustomerSince.ToString("yyyy-MM-dd"),
            accounts
        );
    }
}

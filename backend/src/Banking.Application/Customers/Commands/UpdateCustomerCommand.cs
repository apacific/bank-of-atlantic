using Banking.Application.Abstractions;
using Banking.Application.Common.Errors;
using Banking.Application.Customers.DTOs;
using Banking.Domain.Customers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Banking.Application.Customers.Commands;

public sealed record UpdateCustomerCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string? Suffix,
    string? Title,
    string SsnTin,
    string Email,
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country
) : IRequest<CustomerDetailDto>;

public sealed class UpdateCustomerHandler(IBankingDbContext db)
    : IRequestHandler<UpdateCustomerCommand, CustomerDetailDto>
{
    public async Task<CustomerDetailDto> Handle(UpdateCustomerCommand r, CancellationToken ct)
    {
        var customer = await db.Customers
            .Include(x => x.Accounts)
            .FirstOrDefaultAsync(x => x.Id == r.Id, ct);

        if (customer is null) throw new NotFoundAppException("Customer not found.");

        var emailNorm = r.Email.Trim().ToLowerInvariant();
        var ssnNorm = new string(r.SsnTin.Trim().ToUpperInvariant().Where(char.IsLetterOrDigit).ToArray());

        var conflicts = new Dictionary<string, string[]>();

        if (await db.Customers.AnyAsync(c => c.Id != r.Id && c.EmailNormalized == emailNorm, ct))
            conflicts["Email"] = new[] { "Email is already in use." };

        if (await db.Customers.AnyAsync(c => c.Id != r.Id && c.SsnTinNormalized == ssnNorm, ct))
            conflicts["SsnTin"] = new[] { "SSN/TIN is already in use." };

        if (conflicts.Count > 0)
            throw new ConflictAppException("Duplicate values.", conflicts);

        customer.UpdateProfile(
            r.FirstName,
            r.LastName,
            r.Suffix,
            r.Title,
            r.SsnTin,
            r.Email,
            new Address(r.Street, r.City, r.State, r.PostalCode, r.Country)
        );

        await db.SaveChangesAsync(ct);

        // Return updated
        var accounts = customer.Accounts
            .OrderBy(a => a.AccountNumber)
            .Select(a => new Customers.DTOs.AccountSummaryDto(
                a.Id, a.AccountNumber, a.AccountType.ToString(), a.AvailableBalance))
            .ToList();

        return new Customers.DTOs.CustomerDetailDto(
            customer.Id,
            customer.FirstName,
            customer.LastName,
            customer.Suffix,
            customer.Title,
            customer.SsnTin,
            customer.Email,
            customer.MailingAddress.Street,
            customer.MailingAddress.City,
            customer.MailingAddress.State,
            customer.MailingAddress.PostalCode,
            customer.MailingAddress.Country,
            customer.CustomerSince.ToString("yyyy-MM-dd"),
            accounts
        );
    }
}

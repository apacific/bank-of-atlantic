using Banking.Application.Abstractions;
using Banking.Application.Common.Errors;
using Banking.Application.Customers.DTOs;
using Banking.Domain.Customers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Banking.Application.Customers.Commands;

public sealed record CreateCustomerCommand(
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

public sealed class CreateCustomerHandler(IBankingDbContext db)
    : IRequestHandler<CreateCustomerCommand, CustomerDetailDto>
{
    public async Task<CustomerDetailDto> Handle(CreateCustomerCommand r, CancellationToken ct)
    {
        var emailNorm = r.Email.Trim().ToLowerInvariant();
        var ssnNorm = new string(r.SsnTin.Trim().ToUpperInvariant().Where(char.IsLetterOrDigit).ToArray());

        var conflicts = new Dictionary<string, string[]>();

        if (await db.Customers.AnyAsync(c => c.EmailNormalized == emailNorm, ct))
            conflicts["Email"] = ["Email is already in use."];

        if (await db.Customers.AnyAsync(c => c.SsnTinNormalized == ssnNorm, ct))
            conflicts["SsnTin"] = ["SSN/TIN is already in use."];

        if (conflicts.Count > 0)
            throw new ConflictAppException("Duplicate values.", conflicts);

        var customer = new Customer(
            r.FirstName,
            r.LastName,
            r.Suffix,
            r.Title,
            r.SsnTin,
            r.Email,
            new Address(r.Street, r.City, r.State, r.PostalCode, r.Country),
            DateOnly.FromDateTime(DateTime.UtcNow)
        );

        db.Customers.Add(customer);
        await db.SaveChangesAsync(ct);

        return await new Customers.Queries.GetCustomerByIdHandler(db)
            .Handle(new Customers.Queries.GetCustomerByIdQuery(customer.Id), ct);

    }
}

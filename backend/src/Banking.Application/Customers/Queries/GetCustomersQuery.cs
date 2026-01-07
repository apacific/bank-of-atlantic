using Banking.Application.Abstractions;
using Banking.Application.Customers.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Banking.Application.Customers.Queries;

public sealed record GetCustomersQuery() : IRequest<IReadOnlyList<CustomerListDto>>;

public sealed class GetCustomersHandler(IBankingDbContext db)
    : IRequestHandler<GetCustomersQuery, IReadOnlyList<CustomerListDto>>
{
    public async Task<IReadOnlyList<CustomerListDto>> Handle(GetCustomersQuery request, CancellationToken ct)
    {
        return await db.Customers
            .AsNoTracking()
            .OrderBy(c => c.LastName).ThenBy(c => c.FirstName)
            .Select(c => new CustomerListDto(
                c.Id,
                c.FirstName,
                c.LastName,
                c.CustomerSince.ToString("yyyy-MM-dd")
            ))
            .ToListAsync(ct);
    }
}

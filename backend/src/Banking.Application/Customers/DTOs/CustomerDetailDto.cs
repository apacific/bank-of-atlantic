namespace Banking.Application.Customers.DTOs;

public sealed record CustomerDetailDto(
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
    string Country,
    string CustomerSince,
    IReadOnlyList<AccountSummaryDto> Accounts
);

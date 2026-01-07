namespace Banking.Application.Customers.DTOs;

public sealed record CustomerListDto(
    Guid Id,
    string FirstName,
    string LastName,
    string CustomerSince
);

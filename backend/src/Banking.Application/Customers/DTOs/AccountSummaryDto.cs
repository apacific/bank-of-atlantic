namespace Banking.Application.Customers.DTOs;

public sealed record AccountSummaryDto(
    Guid Id,
    string AccountNumber,
    string AccountType,
    decimal AvailableBalance
);

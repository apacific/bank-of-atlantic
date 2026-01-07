namespace Banking.Application.Accounts.DTOs;

public sealed record AccountDetailDto(
    Guid Id,
    Guid CustomerId,
    string AccountNumber,
    string AccountType,
    string DateOpened,
    decimal AvailableBalance
);

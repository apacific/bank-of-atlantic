namespace Banking.Domain.Accounts;

/// <summary>
/// Represents a bank account belonging to a customer.
/// Aggregates account details including type, number, opening date, and balance.
/// </summary>
public sealed class Account
{
    /// <summary>
    /// Unique identifier for the account.
    /// </summary>
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>
    /// The customer who owns this account.
    /// </summary>
    public Guid CustomerId { get; private set; }

    /// <summary>
    /// The account number (e.g., bank-assigned account identifier).
    /// </summary>
    public string AccountNumber { get; private set; } = "";

    /// <summary>
    /// The type of account (Checking, Savings, Money Market, etc.).
    /// </summary>
    public AccountType AccountType { get; private set; }

    /// <summary>
    /// The date the account was opened.
    /// </summary>
    public DateOnly DateOpened { get; private set; }

    /// <summary>
    /// The current available balance in the account.
    /// </summary>
    public decimal AvailableBalance { get; private set; }

    private Account() { }

    /// <summary>
    /// Creates a new account for a customer.
    /// </summary>
    /// <param name="customerId">The customer ID who owns the account</param>
    /// <param name="accountNumber">The account number</param>
    /// <param name="accountType">The type of account</param>
    /// <param name="dateOpened">The date the account was opened</param>
    public Account(Guid customerId, string accountNumber, AccountType accountType, DateOnly dateOpened)
    {
        CustomerId = customerId;
        AccountNumber = accountNumber;
        AccountType = accountType;
        DateOpened = dateOpened;
        AvailableBalance = 0m;
    }

    /// <summary>
    /// Updates the editable properties of an account.
    /// </summary>
    /// <param name="accountType">The new account type</param>
    /// <param name="availableBalance">The new available balance</param>
    public void UpdateEditable(AccountType accountType, decimal availableBalance)
    {
        AccountType = accountType;
        AvailableBalance = availableBalance;
    }
}

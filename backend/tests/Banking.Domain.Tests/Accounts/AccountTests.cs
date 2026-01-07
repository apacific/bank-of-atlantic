using Banking.Domain.Accounts;
using FluentAssertions;
using Xunit;

namespace Banking.Domain.Tests.Accounts;

/// <summary>
/// Unit tests for the Account aggregate.
/// Verifies domain logic around account creation, balance updates, and account type semantics.
/// </summary>
public class AccountTests
{
    [Fact]
    public void CreateAccount_WithValidParameters_ShouldInitializeCorrectly()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var accountNumber = "ACC-001";
        var accountType = AccountType.Checking;
        var dateOpened = new DateOnly(2025, 1, 15);

        // Act
        var account = new Account(customerId, accountNumber, accountType, dateOpened);

        // Assert
        account.Id.Should().NotBe(Guid.Empty);
        account.CustomerId.Should().Be(customerId);
        account.AccountNumber.Should().Be(accountNumber);
        account.AccountType.Should().Be(accountType);
        account.DateOpened.Should().Be(dateOpened);
        account.AvailableBalance.Should().Be(0m);
    }

    [Theory]
    [InlineData(AccountType.Checking)]
    [InlineData(AccountType.Savings)]
    [InlineData(AccountType.MoneyMarket)]
    [InlineData(AccountType.CD)]
    public void CreateAccount_WithDepositAccountType_ShouldSucceed(AccountType accountType)
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act
        var account = new Account(customerId, "ACC-001", accountType, DateOnly.FromDateTime(DateTime.Now));

        // Assert
        account.AccountType.Should().Be(accountType);
    }

    [Theory]
    [InlineData(AccountType.CreditCard)]
    [InlineData(AccountType.HELOC)]
    [InlineData(AccountType.PLOC)]
    public void CreateAccount_WithCreditAccountType_ShouldSucceed(AccountType accountType)
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act
        var account = new Account(customerId, "ACC-001", accountType, DateOnly.FromDateTime(DateTime.Now));

        // Assert
        account.AccountType.Should().Be(accountType);
    }

    [Fact]
    public void UpdateEditable_WithNewBalance_ShouldUpdateAccountTypeAndBalance()
    {
        // Arrange
        var account = new Account(Guid.NewGuid(), "ACC-001", AccountType.Checking, DateOnly.FromDateTime(DateTime.Now));
        var newAccountType = AccountType.Savings;
        var newBalance = 5000m;

        // Act
        account.UpdateEditable(newAccountType, newBalance);

        // Assert
        account.AccountType.Should().Be(newAccountType);
        account.AvailableBalance.Should().Be(newBalance);
    }

    [Fact]
    public void UpdateEditable_WithZeroBalance_ShouldAcceptZeroValue()
    {
        // Arrange
        var account = new Account(Guid.NewGuid(), "ACC-001", AccountType.Checking, DateOnly.FromDateTime(DateTime.Now));

        // Act
        account.UpdateEditable(AccountType.Savings, 0m);

        // Assert
        account.AvailableBalance.Should().Be(0m);
    }

    [Fact]
    public void UpdateEditable_WithNegativeBalance_ShouldAcceptNegativeValue()
    {
        // Arrange (this tests the current implementation; business rules about negative balances handled at app layer)
        var account = new Account(Guid.NewGuid(), "ACC-001", AccountType.CreditCard, DateOnly.FromDateTime(DateTime.Now));
        var negativeBalance = -5000m;

        // Act
        account.UpdateEditable(AccountType.CreditCard, negativeBalance);

        // Assert
        account.AvailableBalance.Should().Be(negativeBalance);
    }
}

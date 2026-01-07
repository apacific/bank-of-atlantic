using Banking.Api.E2ETests.Pages;
using FluentAssertions;
using OpenQA.Selenium;
using Xunit;

namespace Banking.Api.E2ETests;

public class CustomerE2ETests : BaseTest
{
    private void LoginAsAdmin()
    {
        Navigate("/");
        System.Threading.Thread.Sleep(1000);
        var loginPage = new LoginPage(Driver);
        loginPage.Login("employee@atlantic.local", "Passw0rd!");
        System.Threading.Thread.Sleep(3000); // Wait for navigation
    }

    [Fact]
    public void CustomersPage_ShouldBeAccessibleAfterLogin()
    {
        // Arrange
        LoginAsAdmin();
        var customersPage = new CustomersPage(Driver);

        // Assert
        customersPage.IsPageLoaded().Should().BeTrue(
            because: $"Customers page should load but URL is {Driver.Url}");
    }

    [Fact]
    public void CustomerList_ShouldHaveAtLeastZeroCustomers()
    {
        // Arrange
        LoginAsAdmin();
        var customersPage = new CustomersPage(Driver);
        customersPage.IsPageLoaded().Should().BeTrue();

        // Act
        var count = customersPage.GetCustomerCount();

        // Assert
        count.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void CustomerPage_ShouldRenderTable()
    {
        // Arrange
        LoginAsAdmin();
        var customersPage = new CustomersPage(Driver);

        // Assert
        customersPage.IsPageLoaded().Should().BeTrue();
        customersPage.IsElementPresent(By.CssSelector("div.list")).Should().BeTrue();
    }

    [Fact]
    public void LoginFlow_ShouldNavigateToCustomersUrl()
    {
        // Arrange & Act
        LoginAsAdmin();

        // Assert
        Driver.Url.Should().Contain("/customers",
            because: $"After login, should navigate to /customers but got {Driver.Url}");
    }
}

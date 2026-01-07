using Banking.Api.E2ETests.Pages;
using FluentAssertions;
using OpenQA.Selenium;
using Xunit;

namespace Banking.Api.E2ETests;

public class AuthenticationE2ETests : BaseTest
{
    [Fact]
    public void Login_WithValidCredentials_ShouldNavigateToCustomersPage()
    {
        // Arrange
        Navigate("/");
        System.Threading.Thread.Sleep(1000);
        var loginPage = new LoginPage(Driver);
        loginPage.IsPageLoaded().Should().BeTrue("Login page should be loaded");

        // Act
        loginPage.Login("employee@atlantic.local", "Passw0rd!");
        System.Threading.Thread.Sleep(3000); // Wait for navigation and authentication

        // Assert
        Driver.Url.Should().Contain("/customers", 
            because: $"Expected navigation to /customers but got {Driver.Url}");
    }

    [Fact]
    public void Login_WithInvalidCredentials_ShouldShowErrorModal()
    {
        // Arrange
        Navigate("/");
        System.Threading.Thread.Sleep(1000);
        var loginPage = new LoginPage(Driver);

        // Act
        loginPage.Login("invalid@email.com", "wrongpassword");
        System.Threading.Thread.Sleep(2000);

        // Assert
        loginPage.IsErrorModalDisplayed().Should().BeTrue();
    }

    [Fact]
    public void LoginPage_ShouldHaveProperElements()
    {
        // Arrange
        Navigate("/");
        System.Threading.Thread.Sleep(1000);
        var loginPage = new LoginPage(Driver);

        // Assert
        loginPage.IsPageLoaded().Should().BeTrue();
    }

    [Fact]
    public void Login_ShouldShowFieldsForEmailAndPassword()
    {
        // Arrange
        Navigate("/");
        System.Threading.Thread.Sleep(1000);
        var loginPage = new LoginPage(Driver);

        // Act
        var hasEmailField = loginPage.IsElementPresent(By.CssSelector("input[type='email']"));
        var hasPasswordField = loginPage.IsElementPresent(By.CssSelector("input[type='password']"));

        // Assert
        hasEmailField.Should().BeTrue();
        hasPasswordField.Should().BeTrue();
    }
}

using OpenQA.Selenium;

namespace Banking.Api.E2ETests.Pages;

public class LoginPage : BasePage
{
    private By EmailInput => By.CssSelector("input[type='email']");
    private By PasswordInput => By.CssSelector("input[type='password']");
    private By SignInButton => By.CssSelector("button[type='submit']");
    private By ErrorModal => By.CssSelector(".modal");
    private By ModalMessage => By.CssSelector(".modal .p");
    private By ModalOkButton => By.CssSelector(".modal .btn");
    private By PageTitle => By.CssSelector(".title");

    public LoginPage(IWebDriver driver) : base(driver) { }

    public bool IsPageLoaded() => IsElementPresent(PageTitle) && IsElementPresent(SignInButton);

    public void EnterEmail(string email) => SendKeys(EmailInput, email);

    public void EnterPassword(string password) => SendKeys(PasswordInput, password);

    public void ClickSignIn() => Click(SignInButton);

    public void Login(string email, string password)
    {
        WaitForElement(EmailInput);
        EnterEmail(email);
        EnterPassword(password);
        ClickSignIn();
    }

    public bool IsErrorModalDisplayed() => IsElementPresent(ErrorModal);

    public string GetErrorMessage()
    {
        WaitForElement(ModalMessage);
        return GetText(ModalMessage);
    }

    public void CloseErrorModal() => Click(ModalOkButton);

    public bool IsSignInButtonDisabled()
    {
        var button = FindElement(SignInButton);
        return button.GetAttribute("disabled") != null;
    }

    public string GetSignInButtonText() => GetText(SignInButton);
}

using OpenQA.Selenium;

namespace Banking.Api.E2ETests.Pages;

public class CustomersPage : BasePage
{
    private By PageTitle => By.CssSelector("h1");
    private By CustomerTable => By.CssSelector("div.list");
    private By TableRows => By.CssSelector("div.item");
    private By ModalOverlay => By.CssSelector(".overlay");
    private By FirstNameInput => By.CssSelector("input[placeholder*='First'], input[name*='firstName'], input[name*='first']");
    private By LastNameInput => By.CssSelector("input[placeholder*='Last'], input[name*='lastName'], input[name*='last']");
    private By EmailInput => By.CssSelector("input[type='email']");
    private By SubmitButton => By.CssSelector("button[type='submit']");
    private By CloseButton => By.XPath("//button[contains(text(), 'Close') or contains(text(), 'Cancel') or contains(@class, 'close')]");

    public CustomersPage(IWebDriver driver) : base(driver) { }

    public bool IsPageLoaded()
    {
        try
        {
            WaitForElement(PageTitle);
            var title = GetText(PageTitle);
            return title.Contains("Customer") || title.Contains("customer");
        }
        catch
        {
            return false;
        }
    }

    public void ClickCreateCustomer()
    {
        // Try multiple selectors for Create button
        By[] createButtonSelectors = new[]
        {
            By.XPath("//button[contains(text(), 'Create')]"),
            By.CssSelector("button.btn"),
            By.XPath("//button[1]")
        };

        foreach (var selector in createButtonSelectors)
        {
            try
            {
                Click(selector);
                return;
            }
            catch { }
        }
        throw new Exception("Could not find Create button");
    }

    public int GetCustomerCount()
    {
        try
        {
            var rows = FindElements(TableRows);
            return rows.Count;
        }
        catch
        {
            return 0;
        }
    }

    public void EnterFirstName(string firstName) => SendKeys(FirstNameInput, firstName);

    public void EnterLastName(string lastName) => SendKeys(LastNameInput, lastName);

    public void EnterEmail(string email) => SendKeys(EmailInput, email);

    public void ClickSubmit() => Click(SubmitButton);

    public void CreateCustomer(string firstName, string lastName, string email)
    {
        ClickCreateCustomer();
        WaitForElement(FirstNameInput);
        EnterFirstName(firstName);
        EnterLastName(lastName);
        EnterEmail(email);
        ClickSubmit();
        System.Threading.Thread.Sleep(500);
        try { WaitForElementToDisappear(ModalOverlay); } catch { }
    }

    public bool IsCustomerInList(string email)
    {
        try
        {
            var rows = FindElements(TableRows);
            return rows.Any(row => row.Text.Contains(email));
        }
        catch
        {
            return false;
        }
    }

    public void CloseModal()
    {
        try { Click(CloseButton); } catch { }
    }
}

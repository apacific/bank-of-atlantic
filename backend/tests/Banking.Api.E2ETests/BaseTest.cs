using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace Banking.Api.E2ETests;

public abstract class BaseTest : IDisposable
{
    protected IWebDriver Driver { get; private set; }
    protected WebDriverWait Wait { get; private set; }
    protected string BaseUrl { get; } = "http://localhost:5173";

    public BaseTest()
    {
        var options = new ChromeOptions();
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        // Uncomment for headless mode in CI
        // options.AddArgument("--headless");
        
        Driver = new ChromeDriver(options);
        Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
    }

    protected void Navigate(string path) => Driver.Navigate().GoToUrl($"{BaseUrl}{path}");

    protected IWebElement FindElement(By locator) => Driver.FindElement(locator);

    protected IReadOnlyList<IWebElement> FindElements(By locator) => Driver.FindElements(locator);

    protected void Click(By locator) => Wait.Until(d => d.FindElement(locator)).Click();

    protected void SendKeys(By locator, string text)
    {
        var element = Wait.Until(d => d.FindElement(locator));
        element.Clear();
        element.SendKeys(text);
    }

    protected string GetText(By locator) => FindElement(locator).Text;

    protected bool IsElementPresent(By locator)
    {
        try
        {
            Driver.FindElement(locator);
            return true;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }

    protected void WaitForElement(By locator) => Wait.Until(d => d.FindElement(locator));

    public void Dispose()
    {
        Driver?.Quit();
        Driver?.Dispose();
    }
}

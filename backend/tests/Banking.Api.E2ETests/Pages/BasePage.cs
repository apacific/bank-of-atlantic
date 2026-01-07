using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Diagnostics;

namespace Banking.Api.E2ETests.Pages;

public abstract class BasePage
{
    protected IWebDriver Driver { get; }
    protected WebDriverWait Wait { get; }

    protected BasePage(IWebDriver driver)
    {
        Driver = driver;
        Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
    }

    protected IWebElement FindElement(By locator) => Driver.FindElement(locator);

    protected IReadOnlyList<IWebElement> FindElements(By locator) => Driver.FindElements(locator);

    protected void Click(By locator)
    {
        var element = Wait.Until(d => d.FindElement(locator));
        Wait.Until(d => element.Displayed && element.Enabled);
        element.Click();
    }

    protected void SendKeys(By locator, string text)
    {
        var element = Wait.Until(d => d.FindElement(locator));
        element.Clear();
        element.SendKeys(text);
    }

    protected string GetText(By locator) => FindElement(locator).Text;

    public bool IsElementPresent(By locator)
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

    protected void WaitForElementToDisappear(By locator)
    {
        Wait.Until(d =>
        {
            try
            {
                d.FindElement(locator);
                return false;
            }
            catch (NoSuchElementException)
            {
                return true;
            }
        });
    }

    protected void WaitForUrlToContain(string urlPart)
    {
        Wait.Until(d => d.Url.Contains(urlPart));
    }
}

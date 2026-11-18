using AgenticQa.BddTests.Pages;
using AgenticQa.BddTests.Support;
using Reqnroll;

namespace AgenticQa.BddTests.StepDefinitions;

[Binding]
public sealed class SauceDemoLoginStepDefinitions
{
    private readonly BrowserSessionContext _browserSessionContext;
    private SauceDemoLoginPage? _sauceDemoLoginPage;
    private string _baseUrl = string.Empty;

    public SauceDemoLoginStepDefinitions(BrowserSessionContext browserSessionContext)
    {
        _browserSessionContext = browserSessionContext;
    }

    [Given("the Sauce Demo login page URL is configured")]
    public async Task GivenTheSauceDemoLoginPageUrlIsConfiguredAsync()
    {
        Assert.That(_browserSessionContext.Page, Is.Not.Null, "Playwright page was not initialized.");

        _baseUrl = TestSettingsResolver.ResolveAbsoluteBaseUrl(AppContext.BaseDirectory);
        Assert.That(_baseUrl, Is.Not.Null.And.Not.Empty, "No baseUrl found in appsettings.json.");

        _sauceDemoLoginPage = new SauceDemoLoginPage(_browserSessionContext.Page!);
        await _sauceDemoLoginPage.NavigateAsync(_baseUrl);
    }

    [When("the user logs in to Sauce Demo as {string}")]
    public async Task WhenTheUserLogsInToSauceDemoAsAsync(string username)
    {
        Assert.That(_sauceDemoLoginPage, Is.Not.Null, "Sauce Demo login page was not initialized.");
        var password = Environment.GetEnvironmentVariable("QA_PASSWORD");
        Assert.That(
            password,
            Is.Not.Null.And.Not.Empty,
            "QA_PASSWORD environment variable is required (set it to the provided Sauce Demo password).");

        await _sauceDemoLoginPage!.LoginAsAsync(username, password!);
    }

    [Then("the Sauce Demo page title should be {string}")]
    public async Task ThenTheSauceDemoPageTitleShouldBeAsync(string expectedTitle)
    {
        Assert.That(_sauceDemoLoginPage, Is.Not.Null, "Sauce Demo login page was not initialized.");
        var actualTitle = await _sauceDemoLoginPage!.GetTitleAsync();
        Assert.That(actualTitle, Is.EqualTo(expectedTitle));
    }

    [Then("the user should land on the inventory page")]
    public async Task ThenTheUserShouldLandOnTheInventoryPageAsync()
    {
        Assert.That(_sauceDemoLoginPage, Is.Not.Null, "Sauce Demo login page was not initialized.");
        var isOnInventoryPage = await _sauceDemoLoginPage!.IsOnInventoryPageAsync();
        Assert.That(isOnInventoryPage, Is.True, "Expected successful login to land on /inventory.html.");
    }

    [Then("the Sauce Demo error message should be {string}")]
    public async Task ThenTheSauceDemoErrorMessageShouldBeAsync(string expectedMessage)
    {
        Assert.That(_sauceDemoLoginPage, Is.Not.Null, "Sauce Demo login page was not initialized.");
        var actualMessage = await _sauceDemoLoginPage!.GetErrorMessageAsync();
        Assert.That(actualMessage, Is.EqualTo(expectedMessage));
    }
}

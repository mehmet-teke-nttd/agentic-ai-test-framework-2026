using AgenticQa.BddTests.Pages;
using AgenticQa.BddTests.Support;
using Reqnroll;

namespace AgenticQa.BddTests.StepDefinitions;

[Binding]
public sealed class LoginStepDefinitions
{
    private readonly BrowserSessionContext _browserSessionContext;
    private LoginPage? _loginPage;
    private DashboardPage? _dashboardPage;
    private string _baseUrl = string.Empty;

    public LoginStepDefinitions(BrowserSessionContext browserSessionContext)
    {
        _browserSessionContext = browserSessionContext;
    }

    [Given(@"^the user is on the login page$")]
    public async Task GivenTheUserIsOnTheLoginPageAsync()
    {
        Assert.That(_browserSessionContext.Page, Is.Not.Null, "Playwright page was not initialized.");

        var page = _browserSessionContext.Page!;
        _loginPage = new LoginPage(page);
        _dashboardPage = new DashboardPage(page);
        _baseUrl = TestSettingsResolver.ResolveAbsoluteMockAppBaseUrl(AppContext.BaseDirectory);

        await _loginPage.NavigateAsync(_baseUrl);
    }

    [When(@"^the user logs in with valid credentials$")]
    public async Task WhenTheUserLogsInWithValidCredentialsAsync()
    {
        Assert.That(_loginPage, Is.Not.Null, "Login page was not initialized.");
        var username = Environment.GetEnvironmentVariable("QA_USERNAME");
        var password = Environment.GetEnvironmentVariable("QA_PASSWORD");
        Assert.That(username, Is.Not.Null.And.Not.Empty, "QA_USERNAME environment variable is required.");
        Assert.That(password, Is.Not.Null.And.Not.Empty, "QA_PASSWORD environment variable is required.");

        await _loginPage!.LoginAsAsync(username!, password!);
    }

    [Then(@"^the dashboard should be visible$")]
    public async Task ThenTheDashboardShouldBeVisibleAsync()
    {
        Assert.That(_dashboardPage, Is.Not.Null, "Dashboard page was not initialized.");
        var isVisible = await _dashboardPage!.IsVisibleAsync();
        Assert.That(isVisible, Is.True, "Dashboard should be visible after successful login.");
    }

    [Then(@"^the welcome message should be ""(.*)""$")]
    public async Task ThenTheWelcomeMessageShouldBeAsync(string expectedMessage)
    {
        Assert.That(_dashboardPage, Is.Not.Null, "Dashboard page was not initialized.");
        var message = await _dashboardPage!.GetWelcomeMessageAsync();
        Assert.That(message, Is.EqualTo(expectedMessage));
    }
}

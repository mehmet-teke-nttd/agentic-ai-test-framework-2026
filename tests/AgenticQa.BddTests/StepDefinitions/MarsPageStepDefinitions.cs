using AgenticQa.BddTests.Pages;
using AgenticQa.BddTests.Support;
using Reqnroll;

namespace AgenticQa.BddTests.StepDefinitions;

[Binding]
public sealed class MarsPageStepDefinitions
{
    private readonly BrowserSessionContext _browserSessionContext;
    private MarsHomePage? _marsHomePage;
    private string _marsUrl = string.Empty;

    public MarsPageStepDefinitions(BrowserSessionContext browserSessionContext)
    {
        _browserSessionContext = browserSessionContext;
    }

    [Given("the Mars demo page URL is configured")]
    public void GivenTheMarsDemoPageUrlIsConfigured()
    {
        _marsUrl = TestSettingsResolver.ResolveAbsoluteBaseUrl(AppContext.BaseDirectory);

        Assert.That(_marsUrl, Is.Not.Null.And.Not.Empty, "No baseUrl found in appsettings.json.");
    }

    [When("the user navigates to the Mars demo page")]
    public async Task WhenTheUserNavigatesToTheMarsDemoPage()
    {
        Assert.That(_browserSessionContext.Page, Is.Not.Null, "Playwright page was not initialized.");
        _marsHomePage = new MarsHomePage(_browserSessionContext.Page!);
        await _marsHomePage.NavigateAsync(_marsUrl);
    }

    [Then("the page title should be {string}")]
    public async Task ThenThePageTitleShouldBe(string expectedTitle)
    {
        Assert.That(_marsHomePage, Is.Not.Null, "Mars home page object was not initialized.");
        var title = await _marsHomePage!.GetTitleAsync();
        Assert.That(title, Is.EqualTo(expectedTitle));
    }
}

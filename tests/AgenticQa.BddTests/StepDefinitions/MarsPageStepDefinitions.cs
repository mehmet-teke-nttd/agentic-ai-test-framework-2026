using System.Text.Json;
using AgenticQa.BddTests.Support;
using Reqnroll;

namespace AgenticQa.BddTests.StepDefinitions;

[Binding]
public sealed class MarsPageStepDefinitions
{
    private readonly BrowserSessionContext _browserSessionContext;
    private string _marsUrl = string.Empty;

    public MarsPageStepDefinitions(BrowserSessionContext browserSessionContext)
    {
        _browserSessionContext = browserSessionContext;
    }

    [Given("the Mars demo page URL is configured")]
    public void GivenTheMarsDemoPageUrlIsConfigured()
    {
        var appSettingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        Assert.That(File.Exists(appSettingsPath), Is.True, "appsettings.json was not found in the test output.");

        using var json = JsonDocument.Parse(File.ReadAllText(appSettingsPath));
        var root = json.RootElement;
        if (root.TryGetProperty("testSettings", out var testSettings)
            && testSettings.TryGetProperty("baseUrl", out var nestedBaseUrl)
            && nestedBaseUrl.ValueKind == JsonValueKind.String)
        {
            _marsUrl = nestedBaseUrl.GetString() ?? string.Empty;
        }
        else if (root.TryGetProperty("baseUrl", out var directBaseUrl)
            && directBaseUrl.ValueKind == JsonValueKind.String)
        {
            _marsUrl = directBaseUrl.GetString() ?? string.Empty;
        }

        Assert.That(_marsUrl, Is.Not.Null.And.Not.Empty, "No baseUrl found in appsettings.json.");
    }

    [When("the user navigates to the Mars demo page")]
    public async Task WhenTheUserNavigatesToTheMarsDemoPage()
    {
        Assert.That(_browserSessionContext.Page, Is.Not.Null, "Playwright page was not initialized.");
        await _browserSessionContext.Page!.GotoAsync(_marsUrl);
    }

    [Then("the page title should be {string}")]
    public async Task ThenThePageTitleShouldBe(string expectedTitle)
    {
        Assert.That(_browserSessionContext.Page, Is.Not.Null, "Playwright page was not initialized.");
        var title = await _browserSessionContext.Page!.TitleAsync();
        Assert.That(title, Is.EqualTo(expectedTitle));
    }
}

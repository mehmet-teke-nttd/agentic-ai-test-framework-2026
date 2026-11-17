using Microsoft.Playwright;

namespace AgenticQa.BddTests.Pages;

public sealed class DashboardPage : BasePage
{
    public DashboardPage(IPage page) : base(page)
    {
    }

    private ILocator DashboardContainer => Page.GetByTestId("dashboard");
    private ILocator WelcomeMessage => Page.GetByTestId("welcome-message");

    public async Task<bool> IsVisibleAsync() => await DashboardContainer.IsVisibleAsync();

    public async Task<string?> GetWelcomeMessageAsync() => await WelcomeMessage.TextContentAsync();
}

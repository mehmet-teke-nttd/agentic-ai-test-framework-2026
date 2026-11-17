using Microsoft.Playwright;

namespace AgenticQa.BddTests.Pages;

public sealed class MarsHomePage : BasePage
{
    public MarsHomePage(IPage page) : base(page)
    {
    }

    public Task NavigateAsync(string url) => Page.GotoAsync(url);

    public Task<string> GetTitleAsync() => Page.TitleAsync();
}

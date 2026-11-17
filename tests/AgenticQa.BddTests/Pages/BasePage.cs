using Microsoft.Playwright;

namespace AgenticQa.BddTests.Pages;

public abstract class BasePage
{
    protected BasePage(IPage page)
    {
        Page = page;
    }

    protected IPage Page { get; }
}

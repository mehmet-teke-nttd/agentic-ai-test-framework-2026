using Microsoft.Playwright;

namespace AgenticQa.Core.Interfaces;

public interface IPlaywrightAssertionService
{
    Task AssertVisibleAsync(ILocator locator);
    Task AssertTextAsync(ILocator locator, string expectedText);
    Task AssertUrlAsync(IPage page, string expectedUrl);
}

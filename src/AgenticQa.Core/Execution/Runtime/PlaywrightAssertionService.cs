using AgenticQa.Core.Interfaces;
using Microsoft.Playwright;

namespace AgenticQa.Core.Execution.Runtime;

public sealed class PlaywrightAssertionService : IPlaywrightAssertionService
{
    public async Task AssertVisibleAsync(ILocator locator)
    {
        try
        {
            await Assertions.Expect(locator).ToBeVisibleAsync();
        }
        catch (Exception ex)
        {
            throw new AssertionFailedException("Expected element to be visible.", ex);
        }
    }

    public async Task AssertTextAsync(ILocator locator, string expectedText)
    {
        try
        {
            await Assertions.Expect(locator).ToHaveTextAsync(expectedText);
        }
        catch (Exception ex)
        {
            throw new AssertionFailedException("Expected text did not match.", ex);
        }
    }

    public async Task AssertUrlAsync(IPage page, string expectedUrl)
    {
        try
        {
            await Assertions.Expect(page).ToHaveURLAsync(expectedUrl);
        }
        catch (Exception ex)
        {
            throw new AssertionFailedException("Expected URL did not match.", ex);
        }
    }
}

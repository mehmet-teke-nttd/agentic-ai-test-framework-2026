using Microsoft.Playwright;

namespace AgenticQa.BddTests.Support;

public sealed class BrowserSessionContext
{
    public IPlaywright? Playwright { get; set; }
    public IBrowser? Browser { get; set; }
    public IBrowserContext? BrowserContext { get; set; }
    public IPage? Page { get; set; }
}

using AgenticQa.BddTests.Hooks;
using AgenticQa.BddTests.Support;
using AgenticQa.Core.Enums;
using AgenticQa.Core.Models;
using Microsoft.Playwright;
using NSubstitute;
using Reqnroll.BoDi;

namespace AgenticQa.BddTests.IntegrationTests;

public class HookLifecycleTests
{
    [Test]
    public async Task BrowserHook_CreatesPage_WhenEnvironmentSupportsPlaywright()
    {
        var container = new ObjectContainer();
        var context = new QaScenarioContext();
        var browserContext = new BrowserSessionContext();
        var hooks = new QaFrameworkHooks(container, null, context, browserContext);

        hooks.ConfigureScenarioServices();
        try
        {
            await hooks.CreateBrowserResourcesAsync();
        }
        catch (IgnoreException)
        {
            Assert.Ignore("Playwright runtime unavailable in current environment.");
        }
        finally
        {
            await hooks.CleanupScenarioAsync();
        }

        Assert.That(browserContext.Page, Is.Not.Null);
    }

    [Test]
    public async Task BrowserHook_CleansResources()
    {
        var container = new ObjectContainer();
        var context = new QaScenarioContext
        {
            TestExecutionResult = new TestExecutionResult
            {
                TestId = "TC-LOGIN-001",
                Status = TestStatus.Failed
            }
        };

        var page = Substitute.For<IPage>();
        page.ScreenshotAsync(Arg.Any<PageScreenshotOptions>()).Returns(Task.FromResult(Array.Empty<byte>()));
        page.CloseAsync().Returns(Task.CompletedTask);

        var browserCtx = Substitute.For<IBrowserContext>();
        browserCtx.CloseAsync().Returns(Task.CompletedTask);

        var browser = Substitute.For<IBrowser>();
        browser.CloseAsync().Returns(Task.CompletedTask);

        var session = new BrowserSessionContext
        {
            Page = page,
            BrowserContext = browserCtx,
            Browser = browser
        };

        var hooks = new QaFrameworkHooks(container, null, context, session);
        hooks.ConfigureScenarioServices();

        await hooks.CleanupScenarioAsync();

        await page.Received(1).ScreenshotAsync(Arg.Any<PageScreenshotOptions>());
        await page.Received(1).CloseAsync();
        await browserCtx.Received(1).CloseAsync();
        await browser.Received(1).CloseAsync();
    }
}

using AgenticQa.Core.Enums;
using AgenticQa.Core.Execution.Runtime;
using Microsoft.Extensions.Logging.Abstractions;

namespace AgenticQa.Core.UnitTests.Execution.Runtime;

public class PageRegistryTests
{
    [Test]
    public void KnownPage_ReturnsUrl()
    {
        var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "config", "ui", "page-registry.json");
        var registry = new PageRegistry(path, NullLogger<PageRegistry>.Instance);

        var url = registry.GetUrl("LoginPage");

        Assert.That(url, Is.EqualTo("mock-login.html"));
    }

    [Test]
    public void RelativeUrl_WithBaseUrl_ReturnsAbsoluteUrl()
    {
        var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "config", "ui", "page-registry.json");
        var registry = new PageRegistry(path, NullLogger<PageRegistry>.Instance, "https://app.local/");

        var url = registry.GetUrl("LoginPage");

        Assert.That(url, Is.EqualTo("https://app.local/mock-login.html"));
    }

    [Test]
    public void UnknownPage_ThrowsPageNotRegistered()
    {
        var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "config", "ui", "page-registry.json");
        var registry = new PageRegistry(path, NullLogger<PageRegistry>.Instance);

        var ex = Assert.Throws<PageRegistryException>(() => registry.GetUrl("UnknownPage"));

        Assert.That(ex, Is.Not.Null);
        Assert.That(ex!.ErrorCode, Is.EqualTo(RuntimeExecutionErrorCode.PageNotRegistered));
    }
}

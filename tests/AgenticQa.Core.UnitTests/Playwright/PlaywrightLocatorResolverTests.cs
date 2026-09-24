using AgenticQa.Core.Enums;
using AgenticQa.Core.Models;
using AgenticQa.Core.Playwright;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Playwright;
using NSubstitute;

namespace AgenticQa.Core.UnitTests.Playwright;

public class PlaywrightLocatorResolverTests
{
    private PlaywrightLocatorResolver _resolver = null!;
    private IPage _page = null!;
    private ILocator _locator = null!;

    [SetUp]
    public void Setup()
    {
        _resolver = new PlaywrightLocatorResolver(NullLogger<PlaywrightLocatorResolver>.Instance);
        _page = Substitute.For<IPage>();
        _locator = Substitute.For<ILocator>();
    }

    [Test]
    public void GetByLabel_UsesCorrectStrategy()
    {
        _page.GetByLabel("Username", null).Returns(_locator);

        var resolved = _resolver.Resolve(_page, new ElementRegistryEntry
        {
            Target = "UsernameInput",
            Page = "LoginPage",
            LocatorType = LocatorType.GetByLabel,
            LocatorValue = "Username"
        });

        Assert.That(resolved, Is.SameAs(_locator));
    }

    [Test]
    public void GetByText_UsesCorrectStrategy()
    {
        _page.GetByText("Welcome", null).Returns(_locator);

        var resolved = _resolver.Resolve(_page, new ElementRegistryEntry
        {
            Target = "WelcomeMessage",
            Page = "DashboardPage",
            LocatorType = LocatorType.GetByText,
            LocatorValue = "Welcome"
        });

        Assert.That(resolved, Is.SameAs(_locator));
    }

    [Test]
    public void GetByPlaceholder_UsesCorrectStrategy()
    {
        _page.GetByPlaceholder("Email", null).Returns(_locator);

        var resolved = _resolver.Resolve(_page, new ElementRegistryEntry
        {
            Target = "EmailInput",
            Page = "LoginPage",
            LocatorType = LocatorType.GetByPlaceholder,
            LocatorValue = "Email"
        });

        Assert.That(resolved, Is.SameAs(_locator));
    }

    [Test]
    public void GetByTestId_UsesCorrectStrategy()
    {
        _page.GetByTestId("login-button").Returns(_locator);

        var resolved = _resolver.Resolve(_page, new ElementRegistryEntry
        {
            Target = "LoginButton",
            Page = "LoginPage",
            LocatorType = LocatorType.GetByTestId,
            LocatorValue = "login-button"
        });

        Assert.That(resolved, Is.SameAs(_locator));
    }

    [Test]
    public void Css_UsesCorrectStrategy()
    {
        _page.Locator("#login", null).Returns(_locator);

        var resolved = _resolver.Resolve(_page, new ElementRegistryEntry
        {
            Target = "LoginButton",
            Page = "LoginPage",
            LocatorType = LocatorType.Css,
            LocatorValue = "#login"
        });

        Assert.That(resolved, Is.SameAs(_locator));
    }

    [Test]
    public void GetByRole_UsesCorrectRoleAndName()
    {
        _page.GetByRole(AriaRole.Button, Arg.Any<PageGetByRoleOptions>()).Returns(_locator);

        var resolved = _resolver.Resolve(_page, new ElementRegistryEntry
        {
            Target = "LoginButton",
            Page = "LoginPage",
            LocatorType = LocatorType.GetByRole,
            LocatorValue = "button",
            Name = "Login"
        });

        Assert.That(resolved, Is.SameAs(_locator));
        _page.Received(1).GetByRole(
            AriaRole.Button,
            Arg.Is<PageGetByRoleOptions>(options => options.Name == "Login"));
    }

    [Test]
    public void InvalidAriaRole_ThrowsInvalidLocatorConfig()
    {
        var ex = Assert.Throws<LocatorResolutionException>(() => _resolver.Resolve(_page, new ElementRegistryEntry
        {
            Target = "LoginButton",
            Page = "LoginPage",
            LocatorType = LocatorType.GetByRole,
            LocatorValue = "not-a-role",
            Name = "Login"
        }));

        Assert.That(ex, Is.Not.Null);
        Assert.That(ex!.ErrorCode, Is.EqualTo(LocatorValidationErrorCode.InvalidLocatorConfig));
    }
}

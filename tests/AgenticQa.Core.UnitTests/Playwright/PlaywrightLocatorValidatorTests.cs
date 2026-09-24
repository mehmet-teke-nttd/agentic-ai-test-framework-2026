using AgenticQa.Core.Enums;
using AgenticQa.Core.Models;
using AgenticQa.Core.Playwright;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Playwright;
using NSubstitute;

namespace AgenticQa.Core.UnitTests.Playwright;

public class PlaywrightLocatorValidatorTests
{
    private PlaywrightLocatorValidator _validator = null!;
    private ILocator _locator = null!;
    private ElementRegistryEntry _element = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new PlaywrightLocatorValidator(NullLogger<PlaywrightLocatorValidator>.Instance);
        _locator = Substitute.For<ILocator>();
        _element = new ElementRegistryEntry
        {
            Target = "LoginButton",
            Page = "LoginPage",
            LocatorType = LocatorType.GetByRole,
            LocatorValue = "button",
            Name = "Login"
        };
    }

    [Test]
    public async Task ExpectedPageMatches_AndSingleMatch_ReturnsValid()
    {
        _locator.CountAsync().Returns(1);

        var result = await _validator.ValidateAsync(_locator, _element, "LoginPage");

        Assert.That(result.Status, Is.EqualTo(LocatorValidationStatus.Valid));
        Assert.That(result.MatchCount, Is.EqualTo(1));
        Assert.That(result.Error, Is.Null);
    }

    [Test]
    public async Task WrongPage_ReturnsPageMismatch()
    {
        var result = await _validator.ValidateAsync(_locator, _element, "DashboardPage");

        Assert.That(result.Status, Is.EqualTo(LocatorValidationStatus.Invalid));
        Assert.That(result.Error, Is.Not.Null);
        Assert.That(result.Error!.ErrorCode, Is.EqualTo("PAGE_MISMATCH"));
    }

    [Test]
    public async Task CountZero_ReturnsLocatorNotFound()
    {
        _locator.CountAsync().Returns(0);

        var result = await _validator.ValidateAsync(_locator, _element, "LoginPage");

        Assert.That(result.Status, Is.EqualTo(LocatorValidationStatus.Invalid));
        Assert.That(result.Error, Is.Not.Null);
        Assert.That(result.Error!.ErrorCode, Is.EqualTo("LOCATOR_NOT_FOUND"));
    }

    [Test]
    public async Task CountGreaterThanOne_ReturnsLocatorNotUnique()
    {
        _locator.CountAsync().Returns(2);

        var result = await _validator.ValidateAsync(_locator, _element, "LoginPage");

        Assert.That(result.Status, Is.EqualTo(LocatorValidationStatus.Invalid));
        Assert.That(result.Error, Is.Not.Null);
        Assert.That(result.Error!.ErrorCode, Is.EqualTo("LOCATOR_NOT_UNIQUE"));
    }
}

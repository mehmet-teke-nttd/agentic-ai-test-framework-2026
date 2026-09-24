using AgenticQa.Core.Enums;
using AgenticQa.Core.ElementRegistry;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Logging.Abstractions;
using ElementRegistryModel = AgenticQa.Core.Models.ElementRegistry;

namespace AgenticQa.Core.UnitTests.ElementRegistry;

public class ElementRegistryValidatorTests
{
    private ElementRegistryValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new ElementRegistryValidator(NullLogger<ElementRegistryValidator>.Instance);
    }

    [Test]
    public void ValidRegistry_IsValid()
    {
        var registry = CreateValidRegistry();

        var result = _validator.Validate(registry);

        Assert.That(result.IsValid, Is.True);
        Assert.That(result.Errors, Is.Empty);
    }

    [Test]
    public void DuplicateTarget_IsInvalid()
    {
        var registry = CreateValidRegistry();
        registry.Elements.Add(new ElementRegistryEntry
        {
            Target = "LoginButton",
            Page = "LoginPage",
            LocatorType = LocatorType.Css,
            LocatorValue = "#login"
        });

        var result = _validator.Validate(registry);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.ErrorCode == ElementRegistryValidationErrorCode.DuplicateTarget), Is.True);
    }

    [Test]
    public void MissingTarget_IsInvalid()
    {
        var registry = CreateValidRegistry();
        registry.Elements[0].Target = string.Empty;

        var result = _validator.Validate(registry);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.ErrorCode == ElementRegistryValidationErrorCode.MissingTarget), Is.True);
    }

    [Test]
    public void MissingPage_IsInvalid()
    {
        var registry = CreateValidRegistry();
        registry.Elements[0].Page = string.Empty;

        var result = _validator.Validate(registry);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.ErrorCode == ElementRegistryValidationErrorCode.MissingPage), Is.True);
    }

    [Test]
    public void MissingLocatorValue_IsInvalid()
    {
        var registry = CreateValidRegistry();
        registry.Elements[0].LocatorValue = " ";

        var result = _validator.Validate(registry);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.ErrorCode == ElementRegistryValidationErrorCode.MissingLocatorValue), Is.True);
    }

    [Test]
    public void UnsupportedLocatorType_IsInvalid()
    {
        var registry = CreateValidRegistry();
        registry.Elements[0].LocatorType = (LocatorType)999;

        var result = _validator.Validate(registry);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.ErrorCode == ElementRegistryValidationErrorCode.InvalidLocatorType), Is.True);
    }

    private static ElementRegistryModel CreateValidRegistry() =>
        new()
        {
            Elements =
            [
                new ElementRegistryEntry
                {
                    Target = "UsernameInput",
                    Page = "LoginPage",
                    LocatorType = LocatorType.GetByLabel,
                    LocatorValue = "Username"
                },
                new ElementRegistryEntry
                {
                    Target = "LoginButton",
                    Page = "LoginPage",
                    LocatorType = LocatorType.GetByRole,
                    LocatorValue = "button",
                    Name = "Login"
                }
            ]
        };
}

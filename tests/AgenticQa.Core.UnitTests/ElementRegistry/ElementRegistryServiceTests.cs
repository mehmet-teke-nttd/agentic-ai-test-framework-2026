using AgenticQa.Core.ElementRegistry;
using AgenticQa.Core.Enums;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Logging.Abstractions;
using ElementRegistryModel = AgenticQa.Core.Models.ElementRegistry;

namespace AgenticQa.Core.UnitTests.ElementRegistry;

public class ElementRegistryServiceTests
{
    [Test]
    public void KnownTarget_ReturnsRegistryEntry()
    {
        var service = new ElementRegistryService(
            NullLogger<ElementRegistryService>.Instance,
            new ElementRegistryModel
            {
                Elements =
                [
                    new ElementRegistryEntry
                    {
                        Target = "LoginButton",
                        Page = "LoginPage",
                        LocatorType = LocatorType.GetByRole,
                        LocatorValue = "button",
                        Name = "Login"
                    }
                ]
            });

        var entry = service.GetElement("LoginButton");

        Assert.That(entry.Target, Is.EqualTo("LoginButton"));
    }

    [Test]
    public void UnknownTarget_ThrowsTargetNotRegisteredError()
    {
        var service = new ElementRegistryService(
            NullLogger<ElementRegistryService>.Instance,
            new ElementRegistryModel { Elements = [] });

        var ex = Assert.Throws<ElementLookupException>(() => service.GetElement("DoesNotExist"));
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex!.ErrorCode, Is.EqualTo(LocatorValidationErrorCode.TargetNotRegistered));
    }
}

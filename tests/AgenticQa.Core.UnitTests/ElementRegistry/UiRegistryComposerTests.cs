using AgenticQa.Core.ElementRegistry;
using AgenticQa.Core.Enums;
using AgenticQa.Core.Models;

namespace AgenticQa.Core.UnitTests.ElementRegistry;

public class UiRegistryComposerTests
{
    [Test]
    public void Compose_AddsComponentTargetsWithPageScope()
    {
        var pageRegistry = new AgenticQa.Core.Models.ElementRegistry
        {
            Elements =
            [
                new ElementRegistryEntry
                {
                    Target = "WelcomeMessage",
                    Page = "DashboardPage",
                    LocatorType = LocatorType.GetByTestId,
                    LocatorValue = "welcome-message"
                }
            ]
        };

        var componentRegistry = new ComponentRegistry
        {
            Components =
            [
                new ComponentRegistryEntry
                {
                    Name = "AuthForm",
                    Pages = ["LoginPage"],
                    Elements =
                    [
                        new ComponentElementEntry
                        {
                            Target = "UsernameInput",
                            LocatorType = LocatorType.GetByLabel,
                            LocatorValue = "Username"
                        }
                    ]
                }
            ]
        };

        var composed = UiRegistryComposer.Compose(pageRegistry, componentRegistry);

        Assert.That(composed.Elements.Any(entry => entry.Target == "WelcomeMessage"), Is.True);
        Assert.That(
            composed.Elements.Any(entry =>
                entry.Target == "AuthForm.UsernameInput"
                && entry.Page == "LoginPage"
                && entry.LocatorType == LocatorType.GetByLabel),
            Is.True);
    }
}

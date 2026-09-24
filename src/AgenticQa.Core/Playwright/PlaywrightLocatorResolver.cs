using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace AgenticQa.Core.Playwright;

public sealed class PlaywrightLocatorResolver : ILocatorResolver
{
    private readonly ILogger<PlaywrightLocatorResolver> _logger;

    public PlaywrightLocatorResolver(ILogger<PlaywrightLocatorResolver> logger)
    {
        _logger = logger;
    }

    public ILocator Resolve(IPage page, ElementRegistryEntry element)
    {
        ArgumentNullException.ThrowIfNull(page);
        ArgumentNullException.ThrowIfNull(element);

        ILocator locator = element.LocatorType switch
        {
            Enums.LocatorType.GetByRole => ResolveByRole(page, element),
            Enums.LocatorType.GetByLabel => page.GetByLabel(element.LocatorValue),
            Enums.LocatorType.GetByText => page.GetByText(element.LocatorValue),
            Enums.LocatorType.GetByPlaceholder => page.GetByPlaceholder(element.LocatorValue),
            Enums.LocatorType.GetByTestId => page.GetByTestId(element.LocatorValue),
            Enums.LocatorType.Css => page.Locator(element.LocatorValue),
            _ => throw new InvalidOperationException($"Unsupported locator type: {element.LocatorType}")
        };

        _logger.LogInformation(
            "Locator resolved for target {Target} with type {LocatorType}",
            element.Target,
            element.LocatorType);

        return locator;
    }

    private static ILocator ResolveByRole(IPage page, ElementRegistryEntry element)
    {
        if (!PlaywrightAriaRoleParser.TryParse(element.LocatorValue, out var role))
        {
            throw PlaywrightAriaRoleParser.CreateInvalidRoleException(element.LocatorValue);
        }

        return page.GetByRole(role, new PageGetByRoleOptions
        {
            Name = element.Name
        });
    }
}

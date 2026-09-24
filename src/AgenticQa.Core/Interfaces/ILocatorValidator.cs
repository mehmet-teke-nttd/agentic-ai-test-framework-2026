using AgenticQa.Core.Models;
using Microsoft.Playwright;

namespace AgenticQa.Core.Interfaces;

public interface ILocatorValidator
{
    Task<LocatorValidationResult> ValidateAsync(
        ILocator locator,
        ElementRegistryEntry element,
        string expectedPage);
}

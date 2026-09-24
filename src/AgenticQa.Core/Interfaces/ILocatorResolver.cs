using AgenticQa.Core.Models;
using Microsoft.Playwright;

namespace AgenticQa.Core.Interfaces;

public interface ILocatorResolver
{
    ILocator Resolve(IPage page, ElementRegistryEntry element);
}

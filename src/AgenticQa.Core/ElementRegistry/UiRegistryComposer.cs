using AgenticQa.Core.Models;

namespace AgenticQa.Core.ElementRegistry;

public static class UiRegistryComposer
{
    public static AgenticQa.Core.Models.ElementRegistry Compose(AgenticQa.Core.Models.ElementRegistry pageRegistry, ComponentRegistry? componentRegistry)
    {
        ArgumentNullException.ThrowIfNull(pageRegistry);

        var merged = new AgenticQa.Core.Models.ElementRegistry
        {
            Elements = [.. pageRegistry.Elements]
        };

        if (componentRegistry?.Components is null)
        {
            return merged;
        }

        foreach (var component in componentRegistry.Components)
        {
            if (string.IsNullOrWhiteSpace(component.Name) || component.Elements is null || component.Pages is null)
            {
                continue;
            }

            foreach (var page in component.Pages.Where(static page => !string.IsNullOrWhiteSpace(page)))
            {
                foreach (var element in component.Elements.Where(static element => !string.IsNullOrWhiteSpace(element.Target)))
                {
                    merged.Elements.Add(new ElementRegistryEntry
                    {
                        Target = $"{component.Name}.{element.Target}",
                        Page = page,
                        LocatorType = element.LocatorType,
                        LocatorValue = element.LocatorValue,
                        Name = element.Name
                    });
                }
            }
        }

        return merged;
    }
}

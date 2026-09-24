using AgenticQa.Core.Enums;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Logging;

namespace AgenticQa.Core.ElementRegistry;

public sealed class ElementRegistryService : IElementRegistryService
{
    private readonly ILogger<ElementRegistryService> _logger;
    private readonly Dictionary<string, ElementRegistryEntry> _elementsByTarget;

    public ElementRegistryService(
        ILogger<ElementRegistryService> logger,
        Models.ElementRegistry registry)
    {
        _logger = logger;
        _elementsByTarget = registry.Elements
            .ToDictionary(entry => entry.Target, entry => entry, StringComparer.OrdinalIgnoreCase);
    }

    public ElementRegistryEntry GetElement(string target)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(target);

        _logger.LogInformation("Target lookup started for target {Target}", target);

        if (_elementsByTarget.TryGetValue(target, out var element))
        {
            return element;
        }

        _logger.LogWarning("Target not registered: {Target}", target);
        throw new ElementLookupException(
            LocatorValidationErrorCode.TargetNotRegistered,
            $"Target '{target}' is not registered.");
    }
}

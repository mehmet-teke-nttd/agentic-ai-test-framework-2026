using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Serialization;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AgenticQa.Core.ElementRegistry;

public sealed class ElementRegistryLoader : IElementRegistryLoader
{
    private readonly ILogger<ElementRegistryLoader> _logger;
    private readonly IElementRegistryValidator _validator;
    private readonly object _sync = new();
    private readonly Dictionary<string, Models.ElementRegistry> _cache = new(StringComparer.OrdinalIgnoreCase);

    public ElementRegistryLoader(
        ILogger<ElementRegistryLoader> logger,
        IElementRegistryValidator validator)
    {
        _logger = logger;
        _validator = validator;
    }

    public Models.ElementRegistry Load(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        lock (_sync)
        {
            if (_cache.TryGetValue(filePath, out var cached))
            {
                return cached;
            }
        }

        if (!File.Exists(filePath))
        {
            throw new ElementRegistryConfigurationException(
                $"Element registry file does not exist: {filePath}",
                []);
        }

        var json = File.ReadAllText(filePath);
        Models.ElementRegistry? registry;
        try
        {
            registry = AgenticJsonSerializer.Deserialize<Models.ElementRegistry>(json);
        }
        catch (JsonException ex)
        {
            throw new ElementRegistryConfigurationException(
                $"Element registry JSON is invalid: {ex.Message}",
                []);
        }

        if (registry is null)
        {
            throw new ElementRegistryConfigurationException(
                "Element registry JSON could not be deserialized.",
                []);
        }

        var validation = _validator.Validate(registry);
        if (!validation.IsValid)
        {
            throw new ElementRegistryConfigurationException(
                "Element registry is invalid.",
                validation.Errors);
        }

        lock (_sync)
        {
            _cache[filePath] = registry;
        }

        _logger.LogInformation("Element registry loaded from path {Path}", filePath);
        return registry;
    }
}

using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using AgenticQa.Core.Serialization;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AgenticQa.Core.ElementRegistry;

public sealed class ComponentRegistryLoader : IComponentRegistryLoader
{
    private readonly ILogger<ComponentRegistryLoader> _logger;

    public ComponentRegistryLoader(ILogger<ComponentRegistryLoader> logger)
    {
        _logger = logger;
    }

    public ComponentRegistry Load(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (!File.Exists(filePath))
        {
            throw new ElementRegistryConfigurationException($"Component registry file does not exist: {filePath}", []);
        }

        try
        {
            var json = File.ReadAllText(filePath);
            var registry = AgenticJsonSerializer.Deserialize<ComponentRegistry>(json);
            if (registry?.Components is null || registry.Components.Count == 0)
            {
                throw new ElementRegistryConfigurationException("Component registry must contain at least one component.", []);
            }

            _logger.LogInformation("Component registry loaded from path {Path}", filePath);
            return registry;
        }
        catch (JsonException ex)
        {
            throw new ElementRegistryConfigurationException($"Component registry JSON is invalid: {ex.Message}", []);
        }
    }
}

using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using AgenticQa.Core.Serialization;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AgenticQa.Core.ApiRegistry;

public sealed class ApiEndpointRegistryService : IApiEndpointRegistryService
{
    private readonly ILogger<ApiEndpointRegistryService> _logger;
    private readonly IReadOnlyList<ApiEndpointEntry> _all;
    private readonly Dictionary<string, ApiEndpointEntry> _byName;

    public ApiEndpointRegistryService(ILogger<ApiEndpointRegistryService> logger, string filePath)
    {
        _logger = logger;
        _all = Load(filePath);
        _byName = _all.ToDictionary(endpoint => endpoint.Name, endpoint => endpoint, StringComparer.OrdinalIgnoreCase);
    }

    public ApiEndpointEntry GetEndpoint(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (_byName.TryGetValue(name, out var endpoint))
        {
            return endpoint;
        }

        throw new InvalidOperationException($"API endpoint '{name}' is not registered.");
    }

    public IReadOnlyList<ApiEndpointEntry> GetAll() => _all;

    private IReadOnlyList<ApiEndpointEntry> Load(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (!File.Exists(filePath))
        {
            throw new InvalidOperationException($"API endpoint registry file does not exist: {filePath}");
        }

        try
        {
            var json = File.ReadAllText(filePath);
            var registry = AgenticJsonSerializer.Deserialize<ApiEndpointRegistry>(json);
            if (registry?.Endpoints is null || registry.Endpoints.Count == 0)
            {
                throw new InvalidOperationException("API endpoint registry must contain at least one endpoint.");
            }

            Validate(registry.Endpoints);
            _logger.LogInformation("API endpoint registry loaded from path {Path}", filePath);
            return registry.Endpoints;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"API endpoint registry JSON is invalid: {ex.Message}");
        }
    }

    private static void Validate(IReadOnlyList<ApiEndpointEntry> endpoints)
    {
        foreach (var endpoint in endpoints)
        {
            if (string.IsNullOrWhiteSpace(endpoint.Name) ||
                string.IsNullOrWhiteSpace(endpoint.BasePath) ||
                string.IsNullOrWhiteSpace(endpoint.RelativePath) ||
                string.IsNullOrWhiteSpace(endpoint.Method) ||
                string.IsNullOrWhiteSpace(endpoint.AuthProfile) ||
                string.IsNullOrWhiteSpace(endpoint.SchemaAssertion))
            {
                throw new InvalidOperationException("Each API endpoint entry requires name, basePath, relativePath, method, authProfile, and schemaAssertion.");
            }
        }

        var duplicates = endpoints
            .GroupBy(endpoint => endpoint.Name, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicates.Count > 0)
        {
            throw new InvalidOperationException($"Duplicate API endpoint names are not allowed: {string.Join(", ", duplicates)}");
        }
    }
}

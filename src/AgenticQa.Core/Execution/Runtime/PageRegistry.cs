using AgenticQa.Core.Enums;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using AgenticQa.Core.Serialization;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AgenticQa.Core.Execution.Runtime;

public sealed class PageRegistry : IPageRegistry
{
    private readonly Dictionary<string, string> _urlsByPage;
    private readonly ILogger<PageRegistry> _logger;
    private readonly string? _baseUrl;

    public PageRegistry(string registryPath, ILogger<PageRegistry> logger, string? baseUrl = null)
    {
        _logger = logger;
        _baseUrl = string.IsNullOrWhiteSpace(baseUrl) ? null : baseUrl;
        _urlsByPage = Load(registryPath);
    }

    public string GetUrl(string logicalPage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(logicalPage);

        if (_urlsByPage.TryGetValue(logicalPage, out var url))
        {
            return ToAbsoluteUrl(url);
        }

        throw new PageRegistryException(
            RuntimeExecutionErrorCode.PageNotRegistered,
            $"Logical page '{logicalPage}' is not registered.");
    }

    private Dictionary<string, string> Load(string registryPath)
    {
        if (!File.Exists(registryPath))
        {
            throw new PageRegistryException(
                RuntimeExecutionErrorCode.PageNotRegistered,
                $"Page registry file does not exist: {registryPath}");
        }

        try
        {
            var json = File.ReadAllText(registryPath);
            var config = AgenticJsonSerializer.Deserialize<PageRegistryConfiguration>(json);
            if (config?.Pages is null || config.Pages.Count == 0)
            {
                throw new PageRegistryException(
                    RuntimeExecutionErrorCode.PageNotRegistered,
                    "Page registry must contain at least one page.");
            }

            var dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in config.Pages)
            {
                if (string.IsNullOrWhiteSpace(entry.Page) || string.IsNullOrWhiteSpace(entry.Url))
                {
                    throw new PageRegistryException(
                        RuntimeExecutionErrorCode.PageNotRegistered,
                        "Each page registry entry requires non-empty page and url values.");
                }

                dictionary[entry.Page] = entry.Url;
            }

            _logger.LogInformation("Page registry loaded");
            return dictionary;
        }
        catch (JsonException ex)
        {
            throw new PageRegistryException(
                RuntimeExecutionErrorCode.PageNotRegistered,
                $"Page registry JSON is invalid: {ex.Message}");
        }
    }

    private string ToAbsoluteUrl(string rawUrl)
    {
        if (Uri.TryCreate(rawUrl, UriKind.Absolute, out var absolute))
        {
            return absolute.ToString();
        }

        if (string.IsNullOrWhiteSpace(_baseUrl))
        {
            return rawUrl;
        }

        var baseUri = new Uri(_baseUrl, UriKind.Absolute);
        return new Uri(baseUri, rawUrl).ToString();
    }
}

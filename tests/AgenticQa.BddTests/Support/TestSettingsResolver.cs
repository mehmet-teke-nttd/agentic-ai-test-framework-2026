using AgenticQa.Core.Serialization;
using System.Text.Json;

namespace AgenticQa.BddTests.Support;

public static class TestSettingsResolver
{
    public static BrowserSettings LoadBrowserSettings(string outputRoot)
    {
        var browserSettingsPath = Path.Combine(outputRoot, "browser-settings.json");
        var browserSettingsJson = File.ReadAllText(browserSettingsPath);
        return AgenticJsonSerializer.Deserialize<BrowserSettings>(browserSettingsJson) ?? new BrowserSettings();
    }

    public static string ResolveConfiguredBaseUrl(string outputRoot)
    {
        var browserSettings = LoadBrowserSettings(outputRoot);

        return TryReadAppSetting(outputRoot, "publicBaseUrl")
            ?? TryReadAppSetting(outputRoot, "baseUrl")
            ?? browserSettings.BaseUrl;
    }

    public static string ResolveAbsoluteBaseUrl(string outputRoot)
    {
        var configuredBaseUrl = ResolveConfiguredBaseUrl(outputRoot);
        return ResolveBaseUrl(configuredBaseUrl, outputRoot);
    }

    public static string ResolveAbsoluteMockAppBaseUrl(string outputRoot)
    {
        var browserSettings = LoadBrowserSettings(outputRoot);
        var configuredBaseUrl = TryReadAppSetting(outputRoot, "mockAppBasePath")
            ?? browserSettings.BaseUrl;

        return ResolveBaseUrl(configuredBaseUrl, outputRoot);
    }

    public static string ResolveBaseUrl(string baseUrl, string outputRoot)
    {
        if (Uri.TryCreate(baseUrl, UriKind.Absolute, out var absolute))
        {
            return absolute.ToString();
        }

        var absolutePath = Path.GetFullPath(Path.Combine(outputRoot, baseUrl));
        return new Uri(AppendDirectorySeparator(absolutePath)).ToString();
    }

    private static string AppendDirectorySeparator(string path) =>
        path.EndsWith(Path.DirectorySeparatorChar)
            ? path
            : path + Path.DirectorySeparatorChar;

    private static string? TryReadAppSetting(string outputRoot, string key)
    {
        var appSettingsPath = Path.Combine(outputRoot, "appsettings.json");
        if (!File.Exists(appSettingsPath))
        {
            return null;
        }

        using var json = JsonDocument.Parse(File.ReadAllText(appSettingsPath));
        var root = json.RootElement;

        if (root.TryGetProperty("testSettings", out var testSettings)
            && testSettings.TryGetProperty(key, out var nestedValue)
            && nestedValue.ValueKind == JsonValueKind.String
            && !string.IsNullOrWhiteSpace(nestedValue.GetString()))
        {
            return nestedValue.GetString();
        }

        if (root.TryGetProperty(key, out var directValue)
            && directValue.ValueKind == JsonValueKind.String
            && !string.IsNullOrWhiteSpace(directValue.GetString()))
        {
            return directValue.GetString();
        }

        return null;
    }
}

using AgenticQa.BddTests.Support;
using AgenticQa.Core.DependencyInjection;
using AgenticQa.Core.Enums;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Reqnroll;
using Reqnroll.BoDi;
using System.Text.Json;

namespace AgenticQa.BddTests.Hooks;

[Binding]
public sealed class QaFrameworkHooks
{
    private readonly IObjectContainer _container;
    private readonly ScenarioContext? _scenarioContext;
    private readonly QaScenarioContext _qaScenarioContext;
    private readonly BrowserSessionContext _browserSessionContext;
    private IServiceProvider? _serviceProvider;

    public QaFrameworkHooks(
        IObjectContainer container,
        ScenarioContext? scenarioContext,
        QaScenarioContext qaScenarioContext,
        BrowserSessionContext browserSessionContext)
    {
        _container = container;
        _scenarioContext = scenarioContext;
        _qaScenarioContext = qaScenarioContext;
        _browserSessionContext = browserSessionContext;
    }

    [BeforeScenario(Order = 0)]
    public void ConfigureScenarioServices()
    {
        _qaScenarioContext.ScenarioStartedAt = DateTimeOffset.UtcNow;

        var outputRoot = AppContext.BaseDirectory;
        var elementRegistryPath = Path.Combine(outputRoot, "config", "ui", "element-registry.json");
        var pageRegistryPath = Path.Combine(outputRoot, "config", "ui", "page-registry.json");
        var componentRegistryPath = Path.Combine(outputRoot, "config", "ui", "component-registry.json");
        var apiEndpointRegistryPath = Path.Combine(outputRoot, "config", "api", "endpoint-registry.json");
        var browserSettingsPath = Path.Combine(outputRoot, "browser-settings.json");
        var appSettingsPath = Path.Combine(outputRoot, "appsettings.json");
        var artifactsRoot = Path.GetFullPath(Path.Combine(outputRoot, "..", "..", "..", "..", "artifacts"));
        var resultOutputDirectory = Path.Combine(artifactsRoot, "results");
        var failureAnalysisOutputDirectory = Path.Combine(artifactsRoot, "failure-analysis");

        Directory.CreateDirectory(resultOutputDirectory);
        Directory.CreateDirectory(failureAnalysisOutputDirectory);

        var browserSettingsJson = File.ReadAllText(browserSettingsPath);
        var browserSettings = AgenticJsonSerializer.Deserialize<BrowserSettings>(browserSettingsJson) ?? new BrowserSettings();
        var configuredBaseUrl = ResolveConfiguredBaseUrl(appSettingsPath, browserSettings.BaseUrl);
        var baseUrl = ResolveBaseUrl(configuredBaseUrl, outputRoot);

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAgenticQaPhase8Services(
            elementRegistryPath,
            pageRegistryPath,
            resultOutputDirectory,
            failureAnalysisOutputDirectory,
            componentRegistryPath,
            apiEndpointRegistryPath,
            baseUrl);

        _serviceProvider = services.BuildServiceProvider();

        _container.RegisterInstanceAs(_serviceProvider.GetRequiredService<ITestIntentValidator>());
        _container.RegisterInstanceAs(_serviceProvider.GetRequiredService<IExecutionContractMapper>());
        _container.RegisterInstanceAs(_serviceProvider.GetRequiredService<IExecutionContractValidator>());
        _container.RegisterInstanceAs(_serviceProvider.GetRequiredService<IExecutionEngine>());
        _container.RegisterInstanceAs(_serviceProvider.GetRequiredService<ITestResultWriter>());
        _container.RegisterInstanceAs(_serviceProvider.GetRequiredService<IFailureClassificationService>());
        _container.RegisterInstanceAs(_serviceProvider.GetRequiredService<IFailureAnalysisHistoryService>());
    }

    [BeforeScenario(Order = 100)]
    public async Task CreateBrowserResourcesAsync()
    {
        var outputRoot = AppContext.BaseDirectory;
        var browserSettingsPath = Path.Combine(outputRoot, "browser-settings.json");
        var browserSettings = AgenticJsonSerializer.Deserialize<BrowserSettings>(
            await File.ReadAllTextAsync(browserSettingsPath)) ?? new BrowserSettings();

        try
        {
            var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            _browserSessionContext.Playwright = playwright;

            var launchOptions = new BrowserTypeLaunchOptions
            {
                Headless = browserSettings.Headless
            };

            _browserSessionContext.Browser = browserSettings.Browser.ToLowerInvariant() switch
            {
                "chromium" => await playwright.Chromium.LaunchAsync(launchOptions),
                _ => throw new InvalidOperationException($"Unsupported browser in configuration: {browserSettings.Browser}")
            };

            _browserSessionContext.BrowserContext = await _browserSessionContext.Browser.NewContextAsync();
            _browserSessionContext.Page = await _browserSessionContext.BrowserContext.NewPageAsync();
        }
        catch (Exception ex)
        {
            var requiresStrictSmoke =
                (_scenarioContext?.ScenarioInfo.Tags.Contains("smoke", StringComparer.OrdinalIgnoreCase) ?? false) &&
                string.Equals(Environment.GetEnvironmentVariable("QA_REQUIRE_UI_SMOKE"), "true", StringComparison.OrdinalIgnoreCase);

            if (requiresStrictSmoke)
            {
                Assert.Fail($"Playwright browser initialization failed for mandatory smoke scenario: {ex.Message}");
            }

            Assert.Ignore($"Playwright browser could not be initialized for this environment: {ex.Message}");
        }
    }

    [AfterScenario(Order = 1000)]
    public async Task CleanupScenarioAsync()
    {
        try
        {
            if (_qaScenarioContext.TestExecutionResult?.Status is TestStatus.Failed or TestStatus.Blocked
                && _browserSessionContext.Page is not null)
            {
                var testId = _qaScenarioContext.TestExecutionResult.TestId;
                var artifactsRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "artifacts"));
                var screenshotsDir = Path.Combine(artifactsRoot, "screenshots");
                Directory.CreateDirectory(screenshotsDir);

                var suffix = _qaScenarioContext.TestExecutionResult.Status == TestStatus.Failed
                    ? "failed"
                    : "blocked";
                var fileName = $"{testId}-{suffix}.png";
                var screenshotPath = Path.Combine(screenshotsDir, fileName);
                _qaScenarioContext.ScreenshotPath = screenshotPath;

                await _browserSessionContext.Page.ScreenshotAsync(new PageScreenshotOptions
                {
                    Path = screenshotPath,
                    FullPage = true
                });
            }

            await PersistAgentRunAsync();
        }
        finally
        {
            if (_browserSessionContext.Page is not null)
            {
                await _browserSessionContext.Page.CloseAsync();
            }

            if (_browserSessionContext.BrowserContext is not null)
            {
                await _browserSessionContext.BrowserContext.CloseAsync();
            }

            if (_browserSessionContext.Browser is not null)
            {
                await _browserSessionContext.Browser.CloseAsync();
            }

            _browserSessionContext.Playwright?.Dispose();

            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    private static string ResolveBaseUrl(string baseUrl, string outputRoot)
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

    private static string ResolveConfiguredBaseUrl(string appSettingsPath, string fallbackBaseUrl)
    {
        if (!File.Exists(appSettingsPath))
        {
            return fallbackBaseUrl;
        }

        using var json = JsonDocument.Parse(File.ReadAllText(appSettingsPath));
        var root = json.RootElement;
        if (root.TryGetProperty("testSettings", out var testSettings)
            && testSettings.TryGetProperty("baseUrl", out var nestedBaseUrl)
            && nestedBaseUrl.ValueKind == JsonValueKind.String
            && !string.IsNullOrWhiteSpace(nestedBaseUrl.GetString()))
        {
            return nestedBaseUrl.GetString()!;
        }

        if (root.TryGetProperty("baseUrl", out var directBaseUrl)
            && directBaseUrl.ValueKind == JsonValueKind.String
            && !string.IsNullOrWhiteSpace(directBaseUrl.GetString()))
        {
            return directBaseUrl.GetString()!;
        }

        return fallbackBaseUrl;
    }

    private async Task PersistAgentRunAsync()
    {
        var artifactsRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "artifacts"));
        var agentRunDirectory = Path.Combine(artifactsRoot, "agent-runs");
        var scenarioName = _scenarioContext?.ScenarioInfo.Title ?? "Unnamed scenario";
        var runId = AgentRunRecorder.CreateRunId(scenarioName);
        var analysis = _qaScenarioContext.FailureAnalysisResult;

        var record = new AgentRunRecord
        {
            RunId = runId,
            ScenarioName = scenarioName,
            Tags = _scenarioContext?.ScenarioInfo.Tags?.ToList() ?? [],
            TestId = _qaScenarioContext.TestExecutionResult?.TestId
                ?? _qaScenarioContext.TestIntent?.TestId
                ?? scenarioName,
            TestStatus = _qaScenarioContext.TestExecutionResult?.Status,
            StartedAt = _qaScenarioContext.ScenarioStartedAt,
            CompletedAt = DateTimeOffset.UtcNow,
            Outcome = _scenarioContext?.TestError is null ? "PASSED_OR_SKIPPED" : "FAILED",
            Summary = analysis?.Summary
                ?? _scenarioContext?.TestError?.Message
                ?? "Scenario completed without failure analysis output.",
            Confidence = analysis?.Confidence,
            EscalationRequired = analysis?.EscalationRequired,
            EscalationLevel = analysis?.EscalationLevel,
            EscalationReason = analysis?.EscalationReason,
            RecommendedAction = analysis?.RecommendedAction.ToString(),
            ResultFilePath = _qaScenarioContext.ResultFilePath,
            FailureAnalysisPath = _qaScenarioContext.FailureAnalysisPath,
            ScreenshotPath = _qaScenarioContext.ScreenshotPath
        };

        _qaScenarioContext.AgentRunPath = await AgentRunRecorder.SaveAsync(agentRunDirectory, record);
    }
}

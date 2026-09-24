using AgenticQa.Core.DependencyInjection;
using AgenticQa.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AgenticQa.Core.UnitTests.DependencyInjection;

public class ServiceCollectionExtensionsTests
{
    [Test]
    public void AddAgenticQaPhase4Services_RegistersRequiredServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var registryPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "config", "ui", "element-registry.json");

        services.AddAgenticQaPhase4Services(registryPath);
        var provider = services.BuildServiceProvider();

        Assert.That(provider.GetService<IElementRegistryLoader>(), Is.Not.Null);
        Assert.That(provider.GetService<IElementRegistryValidator>(), Is.Not.Null);
        Assert.That(provider.GetService<IElementRegistryService>(), Is.Not.Null);
        Assert.That(provider.GetService<ILocatorResolver>(), Is.Not.Null);
        Assert.That(provider.GetService<ILocatorValidator>(), Is.Not.Null);
    }

    [Test]
    public void AddAgenticQaPhase5Services_RegistersRequiredServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var elementRegistryPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "config", "ui", "element-registry.json");
        var pageRegistryPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "config", "ui", "page-registry.json");
        var componentRegistryPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "config", "ui", "component-registry.json");

        services.AddAgenticQaPhase5Services(elementRegistryPath, pageRegistryPath, componentRegistryPath);
        var provider = services.BuildServiceProvider();

        Assert.That(provider.GetService<IPageRegistry>(), Is.Not.Null);
        Assert.That(provider.GetService<ITestDataResolver>(), Is.Not.Null);
        Assert.That(provider.GetService<IPlaywrightAssertionService>(), Is.Not.Null);
        Assert.That(provider.GetService<IExecutionCommandHandlerResolver>(), Is.Not.Null);
        Assert.That(provider.GetService<IStepExecutor>(), Is.Not.Null);
    }

    [Test]
    public void AddAgenticQaPhase6Services_RegistersRequiredServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var elementRegistryPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "config", "ui", "element-registry.json");
        var pageRegistryPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "config", "ui", "page-registry.json");
        var componentRegistryPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "config", "ui", "component-registry.json");
        var apiRegistryPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "config", "api", "endpoint-registry.json");
        var resultDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "artifacts", "results");

        services.AddAgenticQaPhase6Services(elementRegistryPath, pageRegistryPath, resultDir, componentRegistryPath, apiRegistryPath);
        var provider = services.BuildServiceProvider();

        Assert.That(provider.GetService<ITestResultAggregator>(), Is.Not.Null);
        Assert.That(provider.GetService<ITestResultWriter>(), Is.Not.Null);
        Assert.That(provider.GetService<IExecutionEngine>(), Is.Not.Null);
        Assert.That(provider.GetService<IApiEndpointRegistryService>(), Is.Not.Null);
    }

    [Test]
    public void AddAgenticQaPhase8Services_RegistersRequiredServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var elementRegistryPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "config", "ui", "element-registry.json");
        var pageRegistryPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "config", "ui", "page-registry.json");
        var componentRegistryPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "config", "ui", "component-registry.json");
        var apiRegistryPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "config", "api", "endpoint-registry.json");
        var resultDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "artifacts", "results");
        var analysisDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "artifacts", "failure-analysis");

        services.AddAgenticQaPhase8Services(elementRegistryPath, pageRegistryPath, resultDir, analysisDir, componentRegistryPath, apiRegistryPath);
        var provider = services.BuildServiceProvider();

        Assert.That(provider.GetService<IFailureAnalysisAgent>(), Is.Not.Null);
        Assert.That(provider.GetService<IFailureClassificationService>(), Is.Not.Null);
        Assert.That(provider.GetService<IFailureAnalysisResponseValidator>(), Is.Not.Null);
        Assert.That(provider.GetService<IFailureAnalysisPromptBuilder>(), Is.Not.Null);
        Assert.That(provider.GetService<IFailureAnalysisHistoryService>(), Is.Not.Null);
    }

    [Test]
    public void AddAgenticQaPhase9Services_RegistersRequiredServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var elementRegistryPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "config", "ui", "element-registry.json");
        var pageRegistryPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "config", "ui", "page-registry.json");
        var componentRegistryPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "config", "ui", "component-registry.json");
        var apiRegistryPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "config", "api", "endpoint-registry.json");
        var resultDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "artifacts", "results");
        var analysisDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "artifacts", "failure-analysis");
        var bugDraftDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "artifacts", "bug-drafts");

        services.AddAgenticQaPhase9Services(
            elementRegistryPath,
            pageRegistryPath,
            resultDir,
            analysisDir,
            bugDraftDir,
            componentRegistryPath,
            apiRegistryPath);
        var provider = services.BuildServiceProvider();

        Assert.That(provider.GetService<IBugRecommendationService>(), Is.Not.Null);
        Assert.That(provider.GetService<IBugDraftBuilder>(), Is.Not.Null);
        Assert.That(provider.GetService<IBugDraftAgent>(), Is.Not.Null);
        Assert.That(provider.GetService<IBugDraftResponseValidator>(), Is.Not.Null);
        Assert.That(provider.GetService<IBugDraftPersistenceService>(), Is.Not.Null);
        Assert.That(provider.GetService<IBugDuplicateChecker>(), Is.Not.Null);
        Assert.That(provider.GetService<IBugTrackerClient>(), Is.Not.Null);
    }

    [Test]
    public void AddAgenticQaPhase10Services_RegistersRequiredServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var elementRegistryPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "config", "ui", "element-registry.json");
        var pageRegistryPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "config", "ui", "page-registry.json");
        var componentRegistryPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "config", "ui", "component-registry.json");
        var apiRegistryPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "config", "api", "endpoint-registry.json");
        var resultDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "artifacts", "results");
        var analysisDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "artifacts", "failure-analysis");
        var bugDraftDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "artifacts", "bug-drafts");
        var bugResultDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "artifacts", "bug-results");

        services.AddAgenticQaPhase10Services(
            elementRegistryPath,
            pageRegistryPath,
            resultDir,
            analysisDir,
            bugDraftDir,
            bugResultDir,
            options =>
            {
                options.OrganizationUrl = "https://dev.azure.com/myorg";
                options.Project = "MyProject";
                options.ApiVersion = "7.1";
                options.PersonalAccessTokenEnvironmentVariable = "AZURE_DEVOPS_PAT";
                options.DryRun = true;
                options.SeverityMapping = new Dictionary<string, string>
                {
                    ["LOW"] = "4 - Low",
                    ["MEDIUM"] = "3 - Medium",
                    ["HIGH"] = "2 - High",
                    ["CRITICAL"] = "1 - Critical"
                };
                options.PriorityMapping = new Dictionary<string, int>
                {
                    ["LOW"] = 3,
                    ["MEDIUM"] = 2,
                    ["HIGH"] = 1
                };
                options.DefaultPriority = 2;
            },
            componentRegistryPath,
            apiRegistryPath);
        var provider = services.BuildServiceProvider();

        Assert.That(provider.GetService<IBugCreationGuard>(), Is.Not.Null);
        Assert.That(provider.GetService<IAzureDevOpsSeverityMapper>(), Is.Not.Null);
        Assert.That(provider.GetService<IAzureDevOpsPriorityMapper>(), Is.Not.Null);
        Assert.That(provider.GetService<IAzureDevOpsBugRequestBuilder>(), Is.Not.Null);
        Assert.That(provider.GetService<IBugCreationResultPersistenceService>(), Is.Not.Null);
        Assert.That(provider.GetService<IBugCreationService>(), Is.Not.Null);
    }
}

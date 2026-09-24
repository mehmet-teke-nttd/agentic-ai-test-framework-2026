using AgenticQa.Core.DependencyInjection;
using AgenticQa.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AgenticQa.BddTests.IntegrationTests;

public class ServiceResolutionTests
{
    [Test]
    public void Phase6Services_ResolveFromDependencyInjection()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var elementRegistryPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "config", "ui", "element-registry.json");
        var pageRegistryPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "config", "ui", "page-registry.json");
        var componentRegistryPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "config", "ui", "component-registry.json");
        var apiRegistryPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "config", "api", "endpoint-registry.json");
        var resultDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "artifacts", "results");
        var baseUrl = new Uri(Path.Combine(TestContext.CurrentContext.TestDirectory, "TestApp") + Path.DirectorySeparatorChar).ToString();

        services.AddAgenticQaPhase6Services(elementRegistryPath, pageRegistryPath, resultDir, componentRegistryPath, apiRegistryPath, baseUrl);
        var provider = services.BuildServiceProvider();

        Assert.That(provider.GetService<ITestIntentValidator>(), Is.Not.Null);
        Assert.That(provider.GetService<IExecutionContractMapper>(), Is.Not.Null);
        Assert.That(provider.GetService<IExecutionContractValidator>(), Is.Not.Null);
        Assert.That(provider.GetService<IExecutionEngine>(), Is.Not.Null);
        Assert.That(provider.GetService<ITestResultWriter>(), Is.Not.Null);
        Assert.That(provider.GetService<IApiEndpointRegistryService>(), Is.Not.Null);
    }
}

using AgenticQa.Core.ElementRegistry;
using Microsoft.Extensions.Logging.Abstractions;

namespace AgenticQa.Core.UnitTests.ElementRegistry;

public class ElementRegistryLoaderTests
{
    [Test]
    public void Load_ValidJson_ReturnsRegistry()
    {
        var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "config", "ui", "element-registry.json");
        var loader = new ElementRegistryLoader(
            NullLogger<ElementRegistryLoader>.Instance,
            new ElementRegistryValidator(NullLogger<ElementRegistryValidator>.Instance));

        var registry = loader.Load(path);

        Assert.That(registry.Elements.Count, Is.GreaterThan(0));
    }

    [Test]
    public void Load_InvalidJson_ThrowsConfigurationException()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}-element-registry.json");
        File.WriteAllText(tempPath, "{ invalid json }");

        try
        {
            var loader = new ElementRegistryLoader(
                NullLogger<ElementRegistryLoader>.Instance,
                new ElementRegistryValidator(NullLogger<ElementRegistryValidator>.Instance));

            Assert.Throws<ElementRegistryConfigurationException>(() => loader.Load(tempPath));
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }
}

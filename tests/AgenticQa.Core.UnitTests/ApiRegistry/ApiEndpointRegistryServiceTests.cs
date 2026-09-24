using AgenticQa.Core.ApiRegistry;
using Microsoft.Extensions.Logging.Abstractions;

namespace AgenticQa.Core.UnitTests.ApiRegistry;

public class ApiEndpointRegistryServiceTests
{
    [Test]
    public void Load_ValidRegistry_ResolvesByName()
    {
        var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "config", "api", "endpoint-registry.json");
        var service = new ApiEndpointRegistryService(NullLogger<ApiEndpointRegistryService>.Instance, path);

        var endpoint = service.GetEndpoint("Login");

        Assert.Multiple(() =>
        {
            Assert.That(endpoint.Method, Is.EqualTo("POST"));
            Assert.That(endpoint.AuthProfile, Is.EqualTo("none"));
            Assert.That(endpoint.SchemaAssertion, Is.EqualTo("schemas/auth-login-response.schema.json"));
        });
    }

    [Test]
    public void GetEndpoint_UnknownName_Throws()
    {
        var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "config", "api", "endpoint-registry.json");
        var service = new ApiEndpointRegistryService(NullLogger<ApiEndpointRegistryService>.Instance, path);

        Assert.Throws<InvalidOperationException>(() => service.GetEndpoint("missing"));
    }
}

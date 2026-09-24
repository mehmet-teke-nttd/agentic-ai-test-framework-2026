using AgenticQa.Core.AzureDevOps;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Options;

namespace AgenticQa.Core.UnitTests.AzureDevOps;

public class AzureDevOpsAuthenticationProviderTests
{
    [Test]
    public void MissingPat_ReturnsConfigurationError()
    {
        const string envName = "AZDO_PAT_TEST";
        Environment.SetEnvironmentVariable(envName, null);
        var provider = new AzureDevOpsAuthenticationProvider(Options.Create(new AzureDevOpsOptions
        {
            PersonalAccessTokenEnvironmentVariable = envName
        }));

        var ok = provider.TryCreateAuthorizationHeader(out _, out var error);

        Assert.That(ok, Is.False);
        Assert.That(error, Does.Contain(envName));
    }
}

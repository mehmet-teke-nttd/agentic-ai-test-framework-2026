using AgenticQa.Core.AzureDevOps;
using AgenticQa.Core.Enums;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Options;

namespace AgenticQa.Core.UnitTests.AzureDevOps.Integration;

[Category("azuredevops")]
[Category("integration")]
public class AzureDevOpsIntegrationSmokeTests
{
    [Test]
    public void AzureDevOpsIntegrationConfig_IsExplicitlyRequired()
    {
        var pat = Environment.GetEnvironmentVariable("AZURE_DEVOPS_PAT");
        var org = Environment.GetEnvironmentVariable("AZURE_DEVOPS_ORG");
        var project = Environment.GetEnvironmentVariable("AZURE_DEVOPS_PROJECT");

        if (string.IsNullOrWhiteSpace(pat) || string.IsNullOrWhiteSpace(org) || string.IsNullOrWhiteSpace(project))
        {
            Assert.Ignore("Azure DevOps integration environment variables are not configured.");
        }

        var provider = new AzureDevOpsAuthenticationProvider(Options.Create(new AzureDevOpsOptions
        {
            OrganizationUrl = org!,
            Project = project!,
            PersonalAccessTokenEnvironmentVariable = "AZURE_DEVOPS_PAT",
            DryRun = true,
            SeverityMapping = new Dictionary<string, string>
            {
                ["LOW"] = "4 - Low",
                ["MEDIUM"] = "3 - Medium",
                ["HIGH"] = "2 - High",
                ["CRITICAL"] = "1 - Critical"
            },
            PriorityMapping = new Dictionary<string, int>
            {
                ["LOW"] = 3,
                ["MEDIUM"] = 2,
                ["HIGH"] = 1
            },
            DefaultPriority = 2
        }));

        var ok = provider.TryCreateAuthorizationHeader(out var header, out _);
        Assert.That(ok, Is.True);
        Assert.That(header, Does.StartWith("Basic "));
    }
}

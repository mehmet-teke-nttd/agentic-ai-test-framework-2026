using AgenticQa.Core.AzureDevOps;
using AgenticQa.Core.Enums;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Options;

namespace AgenticQa.Core.UnitTests.AzureDevOps;

public class AzureDevOpsMappingAndRequestTests
{
    [Test]
    public void SeverityMapping_Works()
    {
        var options = Options.Create(CreateOptions());
        var mapper = new AzureDevOpsSeverityMapper(options);

        var ok = mapper.TryMap(BugSeverity.High, out var value, out _);

        Assert.That(ok, Is.True);
        Assert.That(value, Is.EqualTo("2 - High"));
    }

    [Test]
    public void PriorityMapping_Works()
    {
        var options = Options.Create(CreateOptions());
        var mapper = new AzureDevOpsPriorityMapper(options);

        var ok = mapper.TryMap(BugPriority.Medium, out var value, out _);

        Assert.That(ok, Is.True);
        Assert.That(value, Is.EqualTo(2));
    }

    [Test]
    public void UnassignedPriority_WithoutDefault_Blocks()
    {
        var options = CreateOptions();
        options.DefaultPriority = null;
        var mapper = new AzureDevOpsPriorityMapper(Options.Create(options));

        var ok = mapper.TryMap(BugPriority.Unassigned, out _, out var error);

        Assert.That(ok, Is.False);
        Assert.That(error, Does.Contain("UNASSIGNED"));
    }

    [Test]
    public void RequestBuilder_ProducesJsonPatch_WithExpectedAndActual()
    {
        var options = Options.Create(CreateOptions());
        var severity = new AzureDevOpsSeverityMapper(options);
        var priority = new AzureDevOpsPriorityMapper(options);
        var builder = new AzureDevOpsBugRequestBuilder(options, severity, priority);

        var draft = CreateApprovedDraft();
        var ok = builder.TryBuild(draft, out var request, out var error);

        Assert.That(ok, Is.True, error.Message);
        Assert.That(request.Operations.Any(op => op.Path == "/fields/System.Title" && (string)op.Value! == draft.Draft.Title), Is.True);
        Assert.That(request.Operations.Any(op => op.Path == "/fields/System.Description"), Is.True);
        Assert.That(request.Operations.Any(op => op.Path == "/fields/Microsoft.VSTS.TCM.ReproSteps"), Is.True);

        var repro = request.Operations.Single(op => op.Path == "/fields/Microsoft.VSTS.TCM.ReproSteps").Value?.ToString() ?? string.Empty;
        Assert.That(repro, Does.Contain("Expected Result"));
        Assert.That(repro, Does.Contain("Actual Result"));
    }

    private static AzureDevOpsOptions CreateOptions() =>
        new()
        {
            OrganizationUrl = "https://dev.azure.com/myorg",
            Project = "MyProject",
            ApiVersion = "7.1",
            PersonalAccessTokenEnvironmentVariable = "AZURE_DEVOPS_PAT",
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
            DefaultPriority = null
        };

    private static ApprovedBugDraft CreateApprovedDraft() =>
        new()
        {
            TestId = "TC-LOGIN-001",
            CreationKey = "TC-LOGIN-001:key",
            Review = new BugDraftReview
            {
                TestId = "TC-LOGIN-001",
                Decision = BugDraftReviewDecision.Approved,
                ReviewedBy = "QA",
                Comment = "Approved",
                ReviewedAt = DateTimeOffset.UtcNow
            },
            FailureClassification = FailureClassification.ProductDefect,
            Draft = new BugDraft
            {
                TestId = "TC-LOGIN-001",
                Title = "Valid login does not display Dashboard",
                Summary = "Summary",
                StepsToReproduce = ["Navigate", "Enter credentials", "Click Login"],
                ExpectedResult = "Dashboard should be displayed.",
                ActualResult = "Dashboard was not displayed.",
                Severity = BugSeverity.High,
                Priority = BugPriority.Medium,
                Environment = "UI"
            }
        };
}

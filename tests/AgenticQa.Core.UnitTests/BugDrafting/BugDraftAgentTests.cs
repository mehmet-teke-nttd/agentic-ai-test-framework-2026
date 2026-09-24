using AgenticQa.Core.BugDrafting;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace AgenticQa.Core.UnitTests.BugDrafting;

public class BugDraftAgentTests
{
    [Test]
    public async Task MalformedAiResponse_Rejected()
    {
        var aiProvider = Substitute.For<IAiProvider>();
        aiProvider.GenerateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("not-json");

        var agent = new BugDraftAgent(
            NullLogger<BugDraftAgent>.Instance,
            aiProvider,
            new BugDraftResponseValidator());

        var result = await agent.PolishAsync(CreateRequest());

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task ValidAiResponse_Parsed()
    {
        var aiProvider = Substitute.For<IAiProvider>();
        aiProvider.GenerateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(
                """
                {
                  "title": "Valid login does not display Dashboard",
                  "summary": "Login completed, but dashboard did not appear.",
                  "actualResult": "Dashboard visibility verification failed after Login click."
                }
                """);

        var agent = new BugDraftAgent(
            NullLogger<BugDraftAgent>.Instance,
            aiProvider,
            new BugDraftResponseValidator());

        var result = await agent.PolishAsync(CreateRequest());

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Title, Does.Contain("Dashboard"));
    }

    private static BugDraftRequest CreateRequest() =>
        new()
        {
            TestId = "TC-1",
            Title = "Login failure",
            Actions = ["Step 1", "Step 2"],
            ExpectedResults = ["Dashboard visible"],
            FailedStep = 5,
            ActualError = "ASSERTION_FAILED",
            FailureClassification = Enums.FailureClassification.ProductDefect,
            FailureReviewDecision = Enums.FailureReviewDecision.Confirmed,
            EnvironmentMetadata = "UI",
            EvidenceReferences = ["result.json", "screenshot.png"]
        };
}

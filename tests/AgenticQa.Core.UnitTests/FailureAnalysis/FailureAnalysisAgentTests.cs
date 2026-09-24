using AgenticQa.Core.Enums;
using AgenticQa.Core.FailureAnalysis;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace AgenticQa.Core.UnitTests.FailureAnalysis;

public class FailureAnalysisAgentTests
{
    [Test]
    public async Task ValidAiResponse_ReturnsParsedResult()
    {
        var aiProvider = Substitute.For<IAiProvider>();
        aiProvider.GenerateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(
                """
                {
                  "classification": "AUTOMATION_ISSUE",
                  "confidence": 0.86,
                  "summary": "Locator issue.",
                  "evidence": ["Locator failed"],
                  "recommendedAction": "INVESTIGATE_AUTOMATION"
                }
                """);

        var agent = new FailureAnalysisAgent(
            NullLogger<FailureAnalysisAgent>.Instance,
            aiProvider,
            new FailureAnalysisPromptBuilder(),
            new FailureAnalysisResponseValidator());

        var result = await agent.AnalyzeAsync(CreateRequest("TC-1"));

        Assert.That(result.Classification, Is.EqualTo(FailureClassification.AutomationIssue));
        Assert.That(result.RecommendedAction, Is.EqualTo(FailureRecommendedAction.InvestigateAutomation));
    }

    [Test]
    public async Task InvalidAiResponse_ReturnsUnknown()
    {
        var aiProvider = Substitute.For<IAiProvider>();
        aiProvider.GenerateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("invalid-json");

        var agent = new FailureAnalysisAgent(
            NullLogger<FailureAnalysisAgent>.Instance,
            aiProvider,
            new FailureAnalysisPromptBuilder(),
            new FailureAnalysisResponseValidator());

        var result = await agent.AnalyzeAsync(CreateRequest("TC-2"));

        Assert.That(result.Classification, Is.EqualTo(FailureClassification.Unknown));
    }

    private static FailureAnalysisRequest CreateRequest(string testId) =>
        new()
        {
            TestId = testId,
            Title = "Login test",
            TestType = TestType.UiFunctionalPositive,
            ExpectedResults = ["Dashboard should be visible"],
            FailedStep = new FailureAnalysisStepSnapshot
            {
                Step = 5,
                Keyword = ExecutionKeyword.VerifyVisible,
                Target = "DashboardPage",
                StepStatus = StepStatus.Failed,
                ErrorCode = "ASSERTION_FAILED",
                ErrorMessage = "Not visible"
            }
        };
}

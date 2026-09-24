using AgenticQa.Core.Enums;
using AgenticQa.Core.FailureAnalysis;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace AgenticQa.Core.UnitTests.FailureAnalysis;

public class FailureClassificationServiceTests
{
    private IFailureAnalysisAgent _agent = null!;
    private FailureClassificationService _service = null!;
    private TestIntent _intent = null!;
    private ExecutionContract _contract = null!;

    [SetUp]
    public void Setup()
    {
        _agent = Substitute.For<IFailureAnalysisAgent>();
        _agent.AnalyzeAsync(Arg.Any<FailureAnalysisRequest>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var req = (FailureAnalysisRequest)call[0];
                return new FailureAnalysisResult
                {
                    TestId = req.TestId,
                    Classification = FailureClassification.Unknown,
                    Confidence = 0.33,
                    Summary = "Unknown",
                    Evidence = ["Insufficient evidence"],
                    RecommendedAction = FailureRecommendedAction.QaReviewRequired,
                    EscalationRequired = true,
                    EscalationLevel = FailureEscalationLevel.Medium,
                    EscalationReason = "Insufficient evidence requires QA triage."
                };
            });

        _service = new FailureClassificationService(
            NullLogger<FailureClassificationService>.Instance,
            _agent);

        _intent = new TestIntent
        {
            TestId = "TC-1",
            Title = "Login",
            TestType = TestType.UiFunctionalPositive,
            Preconditions = ["User exists"],
            TestData = [],
            Actions = [new TestAction { Step = 1, Action = "Click Login button" }],
            ExpectedResults = ["Dashboard visible"]
        };

        _contract = new ExecutionContract
        {
            TestId = "TC-1",
            ExecutionType = "UI",
            Steps = [new ExecutionStep { Step = 1, Keyword = ExecutionKeyword.Click, Target = "LoginButton" }]
        };
    }

    [TestCase("LOCATOR_NOT_FOUND")]
    [TestCase("TARGET_NOT_REGISTERED")]
    public async Task LocatorAndTargetErrors_ClassifyAsAutomationIssue(string errorCode)
    {
        var result = await _service.ClassifyIfNeededAsync(_intent, _contract, CreateResult(TestStatus.Blocked, errorCode));

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Classification, Is.EqualTo(FailureClassification.AutomationIssue));
        Assert.That(result.EscalationRequired, Is.False);
    }

    [Test]
    public async Task TestDataReferenceNotFound_ClassifiesAsTestDataIssue()
    {
        var result = await _service.ClassifyIfNeededAsync(_intent, _contract, CreateResult(TestStatus.Blocked, "TEST_DATA_REFERENCE_NOT_FOUND"));

        Assert.That(result!.Classification, Is.EqualTo(FailureClassification.TestDataIssue));
    }

    [Test]
    public async Task NavigationFailed_ClassifiesAsEnvironmentIssue()
    {
        var result = await _service.ClassifyIfNeededAsync(_intent, _contract, CreateResult(TestStatus.Blocked, "NAVIGATION_FAILED"));

        Assert.That(result!.Classification, Is.EqualTo(FailureClassification.EnvironmentIssue));
    }

    [Test]
    public async Task RequirementAmbiguity_ClassifiesAsRequirementIssue()
    {
        var result = await _service.ClassifyIfNeededAsync(
            _intent,
            _contract,
            CreateResult(TestStatus.Blocked, "TEST_INTENT_INVALID", "Requirement conflict detected"));

        Assert.That(result!.Classification, Is.EqualTo(FailureClassification.RequirementIssue));
        Assert.That(result.EscalationRequired, Is.False);
    }

    [Test]
    public async Task UnknownEvidence_ClassifiesAsUnknownThroughAiFallback()
    {
        var result = await _service.ClassifyIfNeededAsync(_intent, _contract, CreateResult(TestStatus.Failed, "ASSERTION_FAILED"));

        Assert.That(result!.Classification, Is.EqualTo(FailureClassification.Unknown));
        Assert.That(result.EscalationRequired, Is.True);
    }

    [Test]
    public async Task DeterministicClassification_AvoidsAiCall()
    {
        await _service.ClassifyIfNeededAsync(_intent, _contract, CreateResult(TestStatus.Blocked, "LOCATOR_NOT_UNIQUE"));

        await _agent.DidNotReceive()
            .AnalyzeAsync(Arg.Any<FailureAnalysisRequest>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task AmbiguousCase_CallsAiProvider()
    {
        await _service.ClassifyIfNeededAsync(_intent, _contract, CreateResult(TestStatus.Failed, "ASSERTION_FAILED"));

        await _agent.Received(1)
            .AnalyzeAsync(Arg.Any<FailureAnalysisRequest>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task PassedResult_DoesNotTriggerAnalysis()
    {
        var result = await _service.ClassifyIfNeededAsync(_intent, _contract, CreateResult(TestStatus.Passed, "NONE"));

        Assert.That(result, Is.Null);
        await _agent.DidNotReceive()
            .AnalyzeAsync(Arg.Any<FailureAnalysisRequest>(), Arg.Any<CancellationToken>());
    }

    private static TestExecutionResult CreateResult(TestStatus status, string errorCode, string? message = null) =>
        new()
        {
            TestId = "TC-1",
            Status = status,
            StartedAt = DateTimeOffset.UtcNow,
            CompletedAt = DateTimeOffset.UtcNow,
            DurationMs = 100,
            TotalSteps = 1,
            PassedSteps = 0,
            FailedSteps = status == TestStatus.Failed ? 1 : 0,
            BlockedSteps = status == TestStatus.Blocked ? 1 : 0,
            SkippedSteps = 0,
            StepResults =
            [
                new StepExecutionResult
                {
                    TestId = "TC-1",
                    Step = 1,
                    Keyword = ExecutionKeyword.VerifyVisible,
                    Target = "DashboardPage",
                    Status = status == TestStatus.Failed ? StepStatus.Failed : StepStatus.Blocked,
                    Error = new ExecutionError
                    {
                        ErrorCode = errorCode,
                        Message = message ?? "error"
                    }
                }
            ]
        };
}
